using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Prefabs & refs")]
    public GameObject cardPrefab;
    public Transform boardParent;
    public BoardManager boardManager;


    [Header("Gameplay Settings")]
    public int matchScore = 100;
    public float mismatchFlipBackDelay = 0.6f;

    [Header("UI")]
    public TMPro.TMP_Text scoreText;

    public int currentMoves;
    public float currentTime;

    Card firstSelected;
    Card secondSelected;
    bool isComparing;

    int currentScore;

    protected override void OnInit() { }

    void Start()
    {
        SaveSystem.Clear(); // clear previous save


        var board = FindObjectOfType<BoardManager>();
        board?.PopulateBoard(cardPrefab, boardParent);

        GameSaveData data = SaveSystem.Load();

        if (data == null)
        {
            StartNewGame();
        }
        else
        {
            LoadFromSave(data);
        }
    }
    public void StartNewGame()
    {
        currentScore = 0;
        currentMoves = 0;
        currentTime = 0f;

        SaveSystem.Clear(); // clear previous save

        boardManager.layout = GetRandomLayout();

        boardManager.ClearBoard(boardParent);
        boardManager.PopulateBoard(cardPrefab, boardParent);
        UpdateUI();

        Debug.Log("New Game Started With Layout → " + boardManager.layout);
    }

    public int bonusCardPoints = 50; // expose in inspector

    public void NotifyCardClicked(Card c)
    {
        if (isComparing)
            return;

        if (c.isMatched || c.isFaceUp)
            return;

        c.SetFace(true);

        // Bonus card immediate behavior
        if (c.isBonusCard || c.cardId == -1)
        {
            // Award bonus points, keep it revealed and mark matched
            currentScore += bonusCardPoints;
            UpdateUI();
            c.MarkMatched();
            Debug.Log($"Bonus collected! +{bonusCardPoints} Score:{currentScore}");
            // small early return, don't treat as pair
            return;
        }

        if (firstSelected == null)
        {
            firstSelected = c;
            return;

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

        yield return new WaitForSeconds(0.25f);

        // If IDs match => matched
        if (firstSelected.cardId == secondSelected.cardId)
        {
            firstSelected.MarkMatched();
            secondSelected.MarkMatched();

            currentScore += matchScore;
            Debug.Log("Match! Score: " + currentScore);

            yield return new WaitForSeconds(0.12f);
        }
        else
        {

            AudioManager.Instance.PlayMismatch();
            StartCoroutine(firstSelected.DoFlip(false));
            yield return StartCoroutine(secondSelected.DoFlip(false));

        }

        // increment moves only once per comparison
        currentMoves++;

        // clear selections
        firstSelected = null;
        secondSelected = null;

        isComparing = false;

        UpdateUI();
        SaveGame();
        CheckForWin();
    }


    void CheckForWin()
    {
        for (int i = 0; i < boardParent.childCount; i++)
        {
            Card c = boardParent.GetChild(i).GetComponent<Card>();
            if (!c.isMatched)
                return; 
        }

        Debug.Log("GAME COMPLETED!");
        AudioManager.Instance.PlayWin();
        HandleGameComplete();
    }

    void HandleGameComplete()
    {
        // ✅ Clear save so old moves are NOT restored
        SaveSystem.Clear();

        // ✅ Small delay for UX (optional)
        Invoke(nameof(RestartLevel), 1.0f);
    }
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        data.layoutType = (int)boardManager.layout;
        data.score = currentScore;
        data.moves = currentMoves;
        data.time = currentTime;

        Transform parent = boardParent;
        int count = parent.childCount;

        data.cardIds = new int[count];
        data.matchedStates = new bool[count];
        data.faceUpStates = new bool[count];

        for (int i = 0; i < count; i++)
        {
            Card c = parent.GetChild(i).GetComponent<Card>();
            data.cardIds[i] = c.cardId;
            data.matchedStates[i] = c.isMatched;
            data.faceUpStates[i] = c.isFaceUp;
        }

        SaveSystem.Save(data);
    }
    void LoadFromSave(GameSaveData data)
    {
        boardManager.layout = (BoardManager.BoardLayout)data.layoutType;

        boardManager.ClearBoard(boardParent);

        currentScore = data.score;
        currentMoves = data.moves;
        currentTime = data.time;

        boardManager.PopulateBoard(cardPrefab, boardParent);

        for (int i = 0; i < boardParent.childCount; i++)
        {
            Card c = boardParent.GetChild(i).GetComponent<Card>();

            c.cardId = data.cardIds[i];
            c.isMatched = data.matchedStates[i];

            if (data.cardIds[i] >= 0)
                c.frontImage.sprite = boardManager.faceSprites[data.cardIds[i]];

            if (data.faceUpStates[i] || c.isMatched)
                c.SetFace(true, instant: true);
            else
                c.SetFace(false, instant: true);
        }
        UpdateUI();

        Debug.Log("Game State Restored");
    }
    BoardManager.BoardLayout GetRandomLayout()
    {
        int rand = Random.Range(0, 3); // 0,1,2

        switch (rand)
        {
            case 0: return BoardManager.BoardLayout.TwoByTwo;
            case 1: return BoardManager.BoardLayout.ThreeByThree;
            default: return BoardManager.BoardLayout.FiveBySix;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

    }
    void RestartLevel()
    {
        StartNewGame();
    }
}
