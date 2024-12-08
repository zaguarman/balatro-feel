using UnityEngine;
using UnityEngine.UI;

public class CardDropZone : UIComponent {
    [SerializeField] protected bool acceptPlayer1Cards = true;
    [SerializeField] protected bool acceptPlayer2Cards = true;
    [SerializeField] protected Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] protected Color validDropColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] protected Color invalidDropColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] protected Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

    protected Image dropZoneImage;
    protected bool isDraggingOverZone;

    protected override void Awake() {
        base.Awake();
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
        bool canAccept = ValidateCardType(card) && (card.IsPlayer1Card() ? acceptPlayer1Cards : acceptPlayer2Cards);
        UpdateVisualFeedback(canAccept);
        return canAccept;
    }

    protected virtual bool ValidateCardType(CardController card) {
        return true; // Base class accepts all card types
    }

    protected virtual void HandleCardDrop(CardController card) {
        // Base implementation does nothing
        // Derived classes should implement their specific card handling logic
    }

    protected virtual void UpdateVisualFeedback(bool isValid) {
        if (dropZoneImage != null) {
            dropZoneImage.color = isDraggingOverZone ?
                (isValid ? validDropColor : invalidDropColor) :
                defaultColor;
        }
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
        UpdateVisualFeedback(false);
    }
}