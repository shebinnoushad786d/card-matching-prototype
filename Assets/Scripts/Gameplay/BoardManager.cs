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

            int id = idList[i]; // now id is the spriteIndex (0..faceSprites.Length-1) or -1 for bonus
            card.cardId = id;               // <-- important: use sprite-based id
            card.isBonusCard = (id == -1);

            if (id >= 0)
                card.frontImage.sprite = faceSprites[id];


            card.backImage.sprite = backSprite;

            card.SetFace(false, instant: true);

            // safe button wiring
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                Card localCard = card;
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
            pairs = slots / 2;
        else
        {
            pairs = (slots - 1) / 2;
            hasBonus = true;
        }

        if (faceSprites == null || faceSprites.Length == 0)
        {
            Debug.LogError("No face sprites assigned!");
            return;
        }

        List<int> availableSpriteIds = new List<int>();

        for (int i = 0; i < faceSprites.Length; i++)
            availableSpriteIds.Add(i);

        Shuffle(availableSpriteIds);

        for (int p = 0; p < pairs; p++)
        {
            int spriteIndex = availableSpriteIds[p % availableSpriteIds.Count];

            ids.Add(spriteIndex);
            ids.Add(spriteIndex);
        }

        // ✅ Bonus for odd grid (3x3)
        if (hasBonus)
            ids.Add(-1);

        // ✅ Final safety check
        if (ids.Count != slots)
            Debug.LogError($"ID generation mismatch! Expected {slots} got {ids.Count}");

        // ✅ Shuffle final positions
        Shuffle(ids);

        if (hasBonus)
            bonusIndex = ids.FindIndex(x => x == -1);
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
