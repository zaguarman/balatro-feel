using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ICardDropZone {
    bool CanAcceptCard(CardController card);
    void OnCardDropped(CardController card);
    RectTransform GetRectTransform();
}

public class CardDropZone : UIComponent, IDropHandler, ICardDropZone, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected bool acceptPlayer1Cards = true;
    [SerializeField] protected bool acceptPlayer2Cards = true;
    [SerializeField] protected Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] protected Color validDropColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] protected Color invalidDropColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] protected Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

    protected Image dropZoneImage;
    protected RectTransform rectTransform;
    protected bool isDraggingOverZone;

    protected override void Awake() {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        SetupVisuals();
    }

    protected virtual void SetupVisuals() {
        dropZoneImage = GetComponent<Image>();
        if (dropZoneImage == null) {
            dropZoneImage = gameObject.AddComponent<Image>();
        }
        dropZoneImage.color = defaultColor;
    }

    public virtual bool CanAcceptCard(CardController card) {
        if (card == null) return false;
        bool canAccept = card.IsPlayer1Card() ? acceptPlayer1Cards : acceptPlayer2Cards;
        if (isDraggingOverZone) {
            UpdateVisualFeedback(canAccept);
        }
        return canAccept;
    }

    protected virtual void UpdateVisualFeedback(bool isValid) {
        if (dropZoneImage != null) {
            dropZoneImage.color = isValid ? validDropColor : invalidDropColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isDraggingOverZone = true;
        if (eventData.pointerDrag != null) {
            var card = eventData.pointerDrag.GetComponent<CardController>();
            if (card != null) {
                UpdateVisualFeedback(CanAcceptCard(card));
            }
        } else {
            dropZoneImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isDraggingOverZone = false;
        dropZoneImage.color = defaultColor;
    }

    public virtual void OnCardDropped(CardController card) {
        if (!CanAcceptCard(card)) {
            dropZoneImage.color = invalidDropColor;
            return;
        }
        HandleCardDrop(card);
        gameMediator?.NotifyGameStateChanged();
        dropZoneImage.color = defaultColor;
    }

    protected virtual void HandleCardDrop(CardController card) {
        // Base implementation
    }

    // Implementing abstract methods from UIComponent
    protected override void RegisterEvents() {
        // No events to register in base drop zone
    }

    protected override void UnregisterEvents() {
        // No events to unregister in base drop zone
    }

    public override void UpdateUI() {
        // Base drop zone doesn't need UI updates
    }

    public static bool IsOverDropZone(Vector3 cardPosition, out ICardDropZone dropZone) {
        dropZone = null;
        var results = new List<RaycastResult>();
        var eventData = new PointerEventData(EventSystem.current) {
            position = Camera.main.WorldToScreenPoint(cardPosition)
        };

        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results) {
            dropZone = result.gameObject.GetComponent<ICardDropZone>();
            if (dropZone != null) {
                return true;
            }
        }

        return false;
    }

    // Implementing IDropHandler
    public void OnDrop(PointerEventData eventData) {
        var card = eventData.pointerDrag?.GetComponent<CardController>();
        if (card != null) {
            OnCardDropped(card);
        }
    }

    // Implementing ICardDropZone
    public RectTransform GetRectTransform() => rectTransform;
}