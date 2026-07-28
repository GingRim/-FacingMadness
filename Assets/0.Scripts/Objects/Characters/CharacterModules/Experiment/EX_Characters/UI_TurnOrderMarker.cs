using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TurnOrderMarker : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI initiativeText;

    private CharacterBase target;
    public CharacterBase Target => target;

    public void SetMarker(CharacterBase character, Sprite portrait, int initiative)
    {
        target = character;

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled =
                portrait != null;
        }

        if (initiativeText != null)
        {
            initiativeText.SetText(
                initiative.ToString());
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        target = null;

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        if (initiativeText != null)
        {
            initiativeText.SetText(string.Empty);
        }

        gameObject.SetActive(false);
    }
}