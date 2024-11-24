using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// The main component that handles drag and drop functionality
public class DraggableCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler {
    private Canvas canvas;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isDragging = false;

    // Events that other components can subscribe to
    public UnityEvent<DraggableCard> OnBeginDragEvent = new UnityEvent<DraggableCard>();
    public UnityEvent<DraggableCard> OnEndDragEvent = new UnityEvent<DraggableCard>();
    public UnityEvent<DraggableCard> OnPointerDownEvent = new UnityEvent<DraggableCard>();
    public UnityEvent<DraggableCard> OnPointerUpEvent = new UnityEvent<DraggableCard>();

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;
        OnBeginDragEvent.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) {
        if (!isDragging) return;

        // Convert screen point to local point within canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        transform.position = canvas.transform.TransformPoint(localPoint);
    }

    public void OnEndDrag(PointerEventData eventData) {
        isDragging = false;
        OnEndDragEvent.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnPointerDownEvent.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnPointerUpEvent.Invoke(this);
    }

    public void ResetPosition() {
        rectTransform.localPosition = originalPosition;
    }
}

// Optional card holder class to manage multiple cards
public class CardHolder : MonoBehaviour {
    private DraggableCard[] cards;

    private void Start() {
        // Get all draggable cards in children
        cards = GetComponentsInChildren<DraggableCard>();

        // Subscribe to events for each card
        foreach (var card in cards) {
            card.OnBeginDragEvent.AddListener(OnCardBeginDrag);
            card.OnEndDragEvent.AddListener(OnCardEndDrag);
        }
    }

    private void OnCardBeginDrag(DraggableCard card) {
        // Bring dragged card to front
        card.transform.SetAsLastSibling();
    }

    private void OnCardEndDrag(DraggableCard card) {
        // Implement any logic for when a card is dropped
        // For example, checking if it's dropped in a valid zone
    }

    private void OnDestroy() {
        // Clean up event listeners
        if (cards != null) {
            foreach (var card in cards) {
                if (card != null) {
                    card.OnBeginDragEvent.RemoveListener(OnCardBeginDrag);
                    card.OnEndDragEvent.RemoveListener(OnCardEndDrag);
                }
            }
        }
    }
}