using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public enum BoardLayout { TwoByTwo, ThreeByThree, FiveBySix }
    public BoardLayout layout = BoardLayout.TwoByTwo;

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

    // simple spawn test (creates empty card instances)
    public void PopulateBoard(GameObject cardPrefab, Transform parent)
    {
        ClearBoard(parent);
        int slots = GetSlotCount();
        for (int i = 0; i < slots; i++)
        {
            var go = Instantiate(cardPrefab, parent);
            var card = go.GetComponent<Card>();
            if (card != null)
            {
                // temporary assign an id for visual testing
                card.cardId = i % 6; // cycle ids so you can see variety later
                card.SetFace(false, instant: true);
            }
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
