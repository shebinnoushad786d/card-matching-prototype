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

    // Called by Button onClick or by BoardManager
    public void OnClicked()
    {
        if (isMatched || isFaceUp) return;
        // delegate selection to GameManager (controller)
        GameManager.Instance.NotifyCardClicked(this);
    }

    // visual flip (placeholder - instant)
    public void SetFace(bool faceUp, bool instant = true)
    {
        isFaceUp = faceUp;
        if (frontImage != null) frontImage.gameObject.SetActive(faceUp);
        if (backImage != null) backImage.gameObject.SetActive(!faceUp);
    }

    public void MarkMatched()
    {
        isMatched = true;
        // disable button so it can't be clicked again
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;
    }
}
