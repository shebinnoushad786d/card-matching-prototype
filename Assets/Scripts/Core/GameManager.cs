using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Prefabs & refs")]
    public GameObject cardPrefab;
    public Transform boardParent;

    protected override void OnInit()
    {
        // initial setup (called once)
    }

    void Start()
    {
        // For now, do a quick sanity test: ensure refs set
        if (cardPrefab == null) Debug.LogWarning("cardPrefab not assigned in GameManager");
        if (boardParent == null) Debug.LogWarning("boardParent not assigned in GameManager");
        FindObjectOfType<BoardManager>()?.PopulateBoard(cardPrefab, boardParent);

    }

    // Called by Card when clicked
    public void NotifyCardClicked(Card c)
    {
        // stub: we'll add selection & match logic in later parts
        Debug.Log($"Card clicked id:{c.cardId} matched:{c.isMatched}");
    }
}
