using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Prefabs & refs")]
    public GameObject cardPrefab;
    public Transform boardParent;

    [Header("Gameplay Settings")]
    public int matchScore = 100;
    public float mismatchFlipBackDelay = 0.6f;

    Card firstSelected;
    Card secondSelected;
    bool isComparing;

    int currentScore;

    protected override void OnInit() { }

    void Start()
    {
        var board = FindObjectOfType<BoardManager>();
        board?.PopulateBoard(cardPrefab, boardParent);
    }

    public int bonusCardPoints = 50; // expose in inspector

    public void NotifyCardClicked(Card c)
    {
        if (isComparing) return;
        if (c.isMatched || c.isFaceUp) return;

        c.SetFace(true);

        // Bonus card immediate behavior
        if (c.isBonusCard || c.cardId == -1)
        {
            // Award bonus points, keep it revealed and mark matched
            currentScore += bonusCardPoints;
            c.MarkMatched();
            Debug.Log($"Bonus collected! +{bonusCardPoints} Score:{currentScore}");
            // small early return, don't treat as pair
            return;
        }

        if (firstSelected == null)
        {
            firstSelected = c;
        }
        else if (secondSelected == null)
        {
            secondSelected = c;
            StartCoroutine(CompareSelected());
        }
    }


    IEnumerator CompareSelected()
    {
        isComparing = true;

        yield return new WaitForSeconds(0.15f);

        if (firstSelected.cardId == secondSelected.cardId)
        {
            // MATCH
            firstSelected.MarkMatched();
            secondSelected.MarkMatched();
            currentScore += matchScore;
            Debug.Log("Match! Score: " + currentScore);
        }
        else
        {
            // MISMATCH
            yield return new WaitForSeconds(mismatchFlipBackDelay);
            firstSelected.SetFace(false);
            secondSelected.SetFace(false);
            Debug.Log("Mismatch!");
        }

        firstSelected = null;
        secondSelected = null;
        isComparing = false;
    }
}
