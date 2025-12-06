using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BoardManager : MonoBehaviour
{
    public enum BoardLayout { TwoByTwo, ThreeByThree, FiveBySix }
    public BoardLayout layout = BoardLayout.TwoByTwo;

    [Header("Art")]
    public Sprite[] faceSprites; // set in inspector
    public Sprite backSprite;

    [Header("Grid")]
    public GridLayoutGroup gridLayoutGroup; // assign board's GridLayoutGroup (optional, auto-find if null)
    public Vector2 cellPadding = new Vector2(8, 8); // inner padding used as buffer if needed

    // Public API: call this to populate the board
    public void PopulateBoard(GameObject cardPrefab, Transform parent)
    {
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = parent.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup == null)
            {
                Debug.LogError("GridLayoutGroup not found on board parent. Please assign.");
                return;
            }
        }

        ClearBoard(parent);

        int slots = GetSlotCount();
        List<int> idList;
        int bonusIndex = -1;
        bool hasBonus = false;

        // generate pair ids and shuffle
        GenerateIdList(slots, out idList, out hasBonus, out bonusIndex);

        // For 3x3, place bonus in center for consistent UI (index 4)
        if (layout == BoardLayout.ThreeByThree && hasBonus)
        {
            int centerIndex = 4; // 0-based index in 3x3
            int currentBonusIdx = idList.FindIndex(x => x == -1);
            if (currentBonusIdx != -1 && currentBonusIdx != centerIndex)
            {
                int tmp = idList[centerIndex];
                idList[centerIndex] = -1;
                idList[currentBonusIdx] = tmp;
                bonusIndex = centerIndex;
            }
        }

        // Adjust grid cell size based on layout & container
        AutoScaleGrid(parent as RectTransform);

        // instantiate cards and assign ids & sprites
        for (int i = 0; i < idList.Count; i++)
        {
            GameObject go = Instantiate(cardPrefab, parent);
            Card card = go.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError("Card prefab is missing Card component!");
                continue;
            }

            int id = idList[i]; // -1 means bonus
            card.cardId = id;
            card.isBonusCard = (id == -1);

            // assign sprites if available
            if (id >= 0 && faceSprites != null && faceSprites.Length > 0)
            {
                int spriteIndex = id % faceSprites.Length; // guarantee in-range
                card.frontImage.sprite = faceSprites[spriteIndex];
            }
            else
            {
                // optional: set a special sprite for bonus or null for placeholder
                card.frontImage.sprite = null;
            }
            card.backImage.sprite = backSprite;

            // ensure visual state is face-down initially
            card.SetFace(false, instant: true);

            // wire button safely using a local copy to avoid closure problems
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                Card localCard = card; // capture local
                btn.onClick.AddListener(() => localCard.OnClicked());
            }
        }

        // DEBUG: print distribution of ids
#if UNITY_EDITOR
        Debug.Log($"Board populated: slots={slots} pairs={(slots / 2)} hasBonus={hasBonus} bonusIndex={bonusIndex}");
        var counts = new Dictionary<int, int>();
        foreach (var id in idList)
        {
            if (!counts.ContainsKey(id)) counts[id] = 0;
            counts[id]++;
        }
        foreach (var kv in counts) Debug.Log($"id:{kv.Key} count:{kv.Value}");
#endif
    }

    void GenerateIdList(int slots, out List<int> ids, out bool hasBonus, out int bonusIndex)
    {
        ids = new List<int>();
        hasBonus = false;
        bonusIndex = -1;

        int pairs;
        if (slots % 2 == 0)
        {
            pairs = slots / 2;
            for (int p = 0; p < pairs; p++)
            {
                // create logical pair id (use p as logical id)
                int logicalId = p;
                ids.Add(logicalId);
                ids.Add(logicalId);
            }
        }
        else
        {
            // odd slot layout (3x3): (slots-1)/2 pairs + 1 bonus
            pairs = (slots - 1) / 2;
            for (int p = 0; p < pairs; p++)
            {
                int logicalId = p;
                ids.Add(logicalId);
                ids.Add(logicalId);
            }
            ids.Add(-1); // bonus marker
            hasBonus = true;
        }

        // Shuffle the id list
        Shuffle(ids);

        // Determine bonus index (if any)
        if (hasBonus)
        {
            bonusIndex = ids.FindIndex(x => x == -1);
        }

        // NOTE: logical ids (0..pairs-1) will be mapped to actual sprite indexes via modulo
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T tmp = list[k];
            list[k] = list[n];
            list[n] = tmp;
        }
    }

    void AutoScaleGrid(RectTransform boardRect)
    {
        if (gridLayoutGroup == null || boardRect == null) return;

        int cols = 2;
        int rows = 2;
        switch (layout)
        {
            case BoardLayout.TwoByTwo:
                cols = 2; rows = 2;
                break;
            case BoardLayout.ThreeByThree:
                cols = 3; rows = 3;
                break;
            case BoardLayout.FiveBySix:
                cols = 6; rows = 5; // treat as 5 rows x 6 columns (5x6)
                break;
        }

        // Set constraint to fixed column count for stable layout
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = cols;

        // compute available width/height
        float paddingX = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float paddingY = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;
        float totalSpacingX = gridLayoutGroup.spacing.x * (cols - 1);
        float totalSpacingY = gridLayoutGroup.spacing.y * (rows - 1);

        float availableWidth = boardRect.rect.width - paddingX - totalSpacingX - cellPadding.x;
        float availableHeight = boardRect.rect.height - paddingY - totalSpacingY - cellPadding.y;

        if (availableWidth <= 0 || availableHeight <= 0)
        {
            // fallback safe size
            gridLayoutGroup.cellSize = new Vector2(100, 100);
            return;
        }

        float cellW = availableWidth / cols;
        float cellH = availableHeight / rows;

        float cellSize = Mathf.Floor(Mathf.Min(cellW, cellH));

        // apply square cell
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
    }

    public int GetSlotCount()
    {
        switch (layout)
        {
            case BoardLayout.TwoByTwo: return 4;
            case BoardLayout.ThreeByThree: return 9;
            case BoardLayout.FiveBySix: return 30;
            default: return 4;
        }
    }

    public void ClearBoard(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}
