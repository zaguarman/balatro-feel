using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDropZone : UIComponent, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected bool acceptPlayer1Cards = true;
    [SerializeField] protected bool acceptPlayer2Cards = true;
    [SerializeField] protected Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] protected Color validDropColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] protected Color invalidDropColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] protected Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

    protected CardDropZoneHandler dropZoneHandler;

    protected override void Awake() {
        base.Awake();
        SetupDropZone();
    }

    protected virtual void SetupDropZone() {
        var dropZoneImage = GetComponent<Image>();
        if (dropZoneImage == null) {
            dropZoneImage = gameObject.AddComponent<Image>();
        }

        dropZoneHandler = new CardDropZoneHandler(
            dropZoneImage,
            defaultColor,
            validDropColor,
            invalidDropColor,
            hoverColor,
            acceptPlayer1Cards,
            acceptPlayer2Cards
        );
    }

    public virtual bool CanAcceptCard(CardController card) {
        return dropZoneHandler.CanAcceptCard(card);
    }

    protected virtual void HandleCardDrop(CardController card) {
        // Base implementation does nothing
        // Derived classes should implement their specific card handling logic
    }

    public void OnDrop(PointerEventData eventData) {
        var card = eventData.pointerDrag?.GetComponent<CardController>();
        if (card != null && CanAcceptCard(card)) {
            HandleCardDrop(card);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        dropZoneHandler.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        dropZoneHandler.OnPointerExit(eventData);
    }

    // Required implementation for UIComponent
    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
        }
    }

    public override void UpdateUI() {
        dropZoneHandler.ResetVisualFeedback();
    }

    protected override void OnDestroy() {
        dropZoneHandler = null;
        base.OnDestroy();
    }
}