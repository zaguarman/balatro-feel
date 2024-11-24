using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

// Modify the existing CardButtonController to include drag functionality
public class CardButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler {
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Card Appearance")]
    [SerializeField] private Vector2 defaultSize = new Vector2(200f, 300f);
    [SerializeField] private float hoverScaleFactor = 1.1f;

    [Header("Drag Settings")]
    [SerializeField] private float dragScaleFactor = 1.2f;
    [SerializeField] private float returnSpeed = 10f;

    private Button button;
    private Image backgroundImage;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private bool isDragging;
    private CardData cardData;
    private bool isPlayer1;

    // Events
    public UnityEvent<CardButtonController> OnBeginDragEvent = new UnityEvent<CardButtonController>();
    public UnityEvent<CardButtonController> OnEndDragEvent = new UnityEvent<CardButtonController>();
    public UnityEvent<CardButtonController> OnCardDropped = new UnityEvent<CardButtonController>();

    private void Awake() {
        InitializeComponents();
        ValidateComponents();
        SetDefaultSize();
    }

    private void InitializeComponents() {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Store the starting position for returning the card
        startPosition = rectTransform.localPosition;
    }

    private void ValidateComponents() {
        if (nameText == null) Debug.LogError($"Name Text component missing on {gameObject.name}");
        if (statsText == null) Debug.LogError($"Stats Text component missing on {gameObject.name}");
        if (descriptionText == null) Debug.LogError($"Description Text component missing on {gameObject.name}");
        if (button == null) Debug.LogError($"Button component missing on {gameObject.name}");
        if (backgroundImage == null) Debug.LogError($"Image component missing on {gameObject.name}");
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;
        startPosition = rectTransform.localPosition;

        // Modify appearance for dragging
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.one * dragScaleFactor;

        OnBeginDragEvent.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) {
        if (!isDragging) return;

        // Convert screen point to local point within canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint)) {
            rectTransform.position = parentCanvas.transform.TransformPoint(localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        isDragging = false;

        // Restore appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localScale = Vector3.one;

        OnEndDragEvent.Invoke(this);
        CheckDropZone();
    }

    private void CheckDropZone() {
        // Here you can implement logic to check if the card was dropped in a valid zone
        // For example, checking if it's over the battlefield area
        // If valid, invoke OnCardDropped, otherwise return to start position
        ReturnToStartPosition();
    }

    private void ReturnToStartPosition() {
        StartCoroutine(SmoothReturn());
    }

    private System.Collections.IEnumerator SmoothReturn() {
        float elapsedTime = 0;
        Vector3 currentPos = rectTransform.localPosition;

        while (elapsedTime < 1f) {
            elapsedTime += Time.deltaTime * returnSpeed;
            rectTransform.localPosition = Vector3.Lerp(currentPos, startPosition, elapsedTime);
            yield return null;
        }

        rectTransform.localPosition = startPosition;
    }

    // Implementation of inherited interface methods
    public void OnPointerEnter(PointerEventData eventData) {
        if (!isDragging) {
            transform.localScale = Vector3.one * hoverScaleFactor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!isDragging) {
            transform.localScale = Vector3.one;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isDragging) {
            // Handle click events here
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        // Any pre-drag initialization can go here
    }

    // Original setup methods
    public void Setup(CardData data, bool isPlayerOne) {
        cardData = data;
        isPlayer1 = isPlayerOne;

        UpdateCardContent();
        UpdateCardColor();
    }

    private void UpdateCardContent() {
        if (cardData == null) return;

        if (nameText != null) {
            nameText.text = cardData.cardName;
        }

        if (statsText != null) {
            if (cardData is CreatureData creatureData) {
                statsText.gameObject.SetActive(true);
                statsText.text = $"{creatureData.attack} / {creatureData.health}";
            } else {
                statsText.gameObject.SetActive(false);
            }
        }

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

    private void SetDefaultSize() {
        if (rectTransform != null) {
            rectTransform.sizeDelta = defaultSize;
        }
    }

    public void SetInteractable(bool interactable) {
        if (button != null) {
            button.interactable = interactable;
        }
    }

    public CardData GetCardData() => cardData;
    public bool IsPlayer1Card() => isPlayer1;
}