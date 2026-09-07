using UnityEngine;
using UnityEngine.UI;

public class UI_FieldCharacterMarker : MonoBehaviour
{
    private static readonly Vector2 RuntimeMarkerSize = new Vector2(56f, 56f);

    [SerializeField]
    private Image characterImage;

    private CharacterBase character;

    public CharacterBase Character => character;

    /// <summary>
    /// 별도 마커 프리팹이 없어도 캐릭터 아이콘을 표시할 수 있는
    /// 기본 UI 마커를 런타임에 생성합니다.
    /// </summary>
    public static UI_FieldCharacterMarker CreateRuntime(Transform parent)
    {
        GameObject markerObject = new GameObject(
            "FieldCharacterMarker",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        if (parent != null)
        {
            markerObject.layer = parent.gameObject.layer;
            markerObject.transform.SetParent(parent, false);
        }

        RectTransform rectTransform =
            markerObject.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = RuntimeMarkerSize;

        Image image = markerObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;

        UI_FieldCharacterMarker marker =
            markerObject.AddComponent<UI_FieldCharacterMarker>();

        marker.characterImage = image;
        markerObject.SetActive(false);

        return marker;
    }

    public void SetCharacter(CharacterBase newCharacter)
    {
        character = newCharacter;

        if (characterImage != null)
        {
            characterImage.sprite = character != null ? character.Icon : null;
            characterImage.color = characterImage.sprite != null
                ? Color.white
                : new Color(0.2f, 0.9f, 1f, 1f);
            characterImage.enabled = character != null;
        }

        gameObject.SetActive(character != null);
    }

    public void MoveToNode(FieldNode node)
    {
        if (node == null)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.SetParent(node.MarkerRoot, false);

        if (transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        transform.SetAsLastSibling();

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        character = null;

        if (characterImage != null)
        {
            characterImage.sprite = null;
            characterImage.enabled = false;
        }

        gameObject.SetActive(false);
    }
}
