using System;
using TMPro;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UI_FieldEventChoiceButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI choiceText;

    private Button button;
    private int choiceIndex;

    private Action<int> onSelected;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.RemoveListener(HandleClick);

        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);
    }

    public void SetChoice(int index, FieldEventChoice choice, Action<int> selectedCallback)
    {
        choiceIndex = index;
        onSelected = selectedCallback;

        if (choiceText != null)
        {
            choiceText.SetText(choice != null ? choice.ChoiceText : string.Empty);
        }

        button.interactable = choice != null;

        gameObject.SetActive(choice != null);
    }

    public void Clear()
    {
        choiceIndex = -1;
        onSelected = null;

        if (choiceText != null)
        {
            choiceText.SetText(string.Empty);
        }

        if (button != null)
        {
            button.interactable = false;
        }

        gameObject.SetActive(false);
    }

    private void HandleClick()
    {
        if (choiceIndex < 0)
            return;

        onSelected?.Invoke(choiceIndex);
    }
}
