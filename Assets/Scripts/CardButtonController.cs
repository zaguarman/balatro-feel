using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Card Appearance")]
    [SerializeField] private Vector2 defaultSize = new Vector2(200f, 300f);
    [SerializeField] private float hoverScaleFactor = 1.1f;

    private Button button;
    private Image backgroundImage;
    private RectTransform rectTransform;
    private CardData cardData;
    private bool isPlayer1;

    private void Awake() {
        InitializeComponents();
        ValidateComponents();
        SetDefaultSize();
    }

    private void InitializeComponents() {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void ValidateComponents() {
        if (nameText == null) Debug.LogError($"Name Text component missing on {gameObject.name}");
        if (statsText == null) Debug.LogError($"Stats Text component missing on {gameObject.name}");
        if (descriptionText == null) Debug.LogError($"Description Text component missing on {gameObject.name}");
        if (button == null) Debug.LogError($"Button component missing on {gameObject.name}");
        if (backgroundImage == null) Debug.LogError($"Image component missing on {gameObject.name}");
    }

    private void SetDefaultSize() {
        if (rectTransform != null) {
            rectTransform.sizeDelta = defaultSize;
        }
    }

    public void Setup(CardData data, bool isPlayerOne) {
        cardData = data;
        isPlayer1 = isPlayerOne;

        UpdateCardContent();
        UpdateCardColor();
    }

    private void UpdateCardContent() {
        if (cardData == null) return;

        // Update name
        if (nameText != null) {
            nameText.text = cardData.cardName;
        }

        // Update stats for creature cards
        if (statsText != null) {
            if (cardData is CreatureData creatureData) {
                statsText.gameObject.SetActive(true);
                statsText.text = $"{creatureData.attack} / {creatureData.health}";
            } else {
                statsText.gameObject.SetActive(false);
            }
        }

        // Update description
        if (descriptionText != null) {
            descriptionText.text = cardData.description;
        }
    }

    private void UpdateCardColor() {
        if (backgroundImage != null) {
            Color cardColor = isPlayer1
                ? GameReferences.Instance.GetPlayer1CardColor()
                : GameReferences.Instance.GetPlayer2CardColor();
            backgroundImage.color = cardColor;
        }
    }

    public void SetInteractable(bool interactable) {
        if (button != null) {
            button.interactable = interactable;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        transform.localScale = Vector3.one * hoverScaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        transform.localScale = Vector3.one;
    }

    public CardData GetCardData() => cardData;

    public bool IsPlayer1Card() => isPlayer1;
}