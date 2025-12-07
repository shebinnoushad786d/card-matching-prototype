using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("View refs")]
    public Image frontImage;
    public Image backImage;

    [Header("Model")]
    public int cardId = -1;
    public bool isMatched = false;
    public bool isFaceUp = false;
    public bool isBonusCard = false;

    [Header("Flip")]
    public float flipDuration = 0.22f;
    bool isAnimating = false;
    Coroutine runningFlip = null;

    // Called by Button onClick or by BoardManager
    public void OnClicked()
    {
        if (isMatched || isFaceUp || isAnimating) return;
        AudioManager.Instance.PlayFlip();
        GameManager.Instance.NotifyCardClicked(this);
    }

    // Instant or animated set (does not wait)
    public void SetFace(bool show, bool instant = false)
    {
        // stop any running flip if instant requested
        if (instant)
        {
            if (runningFlip != null)
            {
                StopCoroutine(runningFlip);
                runningFlip = null;
                isAnimating = false;
                transform.localScale = Vector3.one;
            }

            isFaceUp = show;
            frontImage.gameObject.SetActive(show);
            backImage.gameObject.SetActive(!show);
            return;
        }

        // if a flip is running, stop it so new flip can start fresh
        if (runningFlip != null)
        {
            StopCoroutine(runningFlip);
            runningFlip = null;
            isAnimating = false;
            transform.localScale = Vector3.one;
        }

        runningFlip = StartCoroutine(FlipRoutine(show));
    }

    // Awaitable flip: GameManager will yield return DoFlip(...)
    public IEnumerator DoFlip(bool show)
    {
        // If a flip is running, stop it to avoid conflicts
        if (runningFlip != null)
        {
            StopCoroutine(runningFlip);
            runningFlip = null;
            isAnimating = false;
            transform.localScale = Vector3.one;
        }

        // Run flip and yield until finished
        yield return StartCoroutine(FlipRoutine(show));
    }

    // The actual flip animation coroutine (private)
    IEnumerator FlipRoutine(bool show)
    {
        isAnimating = true;
        float half = Mathf.Max(0.01f, flipDuration * 0.5f);
        float t = 0f;

        // first half: scale X 1 -> 0
        while (t < half)
        {
            t += Time.deltaTime;
            float x = Mathf.Lerp(1f, 0f, t / half);
            transform.localScale = new Vector3(x, 1f, 1f);
            yield return null;
        }

        // ensure closed
        transform.localScale = new Vector3(0f, 1f, 1f);

        // swap face
        isFaceUp = show;
        frontImage.gameObject.SetActive(show);
        backImage.gameObject.SetActive(!show);

        // second half: scale X 0 -> 1
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float x = Mathf.Lerp(0f, 1f, t / half);
            transform.localScale = new Vector3(x, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        isAnimating = false;
        runningFlip = null;
    }

    public void MarkMatched()
    {
        isMatched = true;
        AudioManager.Instance.PlayMatch();
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;
    }
}
