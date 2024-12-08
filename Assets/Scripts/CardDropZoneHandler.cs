using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDropZoneHandler {
    private readonly Image dropZoneImage;
    private bool isDraggingOverZone;
    private readonly Color defaultColor;
    private readonly Color validDropColor;
    private readonly Color invalidDropColor;
    private readonly Color hoverColor;
    private readonly bool acceptPlayer1Cards;
    private readonly bool acceptPlayer2Cards;

    public CardDropZoneHandler(
        Image dropZoneImage,
        Color defaultColor,
        Color validDropColor,
        Color invalidDropColor,
        Color hoverColor,
        bool acceptPlayer1Cards = true,
        bool acceptPlayer2Cards = true) {
        this.dropZoneImage = dropZoneImage;
        this.defaultColor = defaultColor;
        this.validDropColor = validDropColor;
        this.invalidDropColor = invalidDropColor;
        this.hoverColor = hoverColor;
        this.acceptPlayer1Cards = acceptPlayer1Cards;
        this.acceptPlayer2Cards = acceptPlayer2Cards;

        // Set initial color
        if (dropZoneImage != null) {
            dropZoneImage.color = defaultColor;
        }
    }

    public virtual bool CanAcceptCard(CardController card) {
        if (card == null) return false;
        bool canAccept = ValidateCardType(card) && (card.IsPlayer1Card() ? acceptPlayer1Cards : acceptPlayer2Cards);
        UpdateVisualFeedback(canAccept);
        return canAccept;
    }

    protected virtual bool ValidateCardType(CardController card) {
        return true;
    }

    public void UpdateVisualFeedback(bool isValid) {
        if (dropZoneImage != null) {
            dropZoneImage.color = isDraggingOverZone ?
                (isValid ? validDropColor : invalidDropColor) :
                defaultColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isDraggingOverZone = true;
        if (eventData.pointerDrag != null) {
            var card = eventData.pointerDrag.GetComponent<CardController>();
            if (card != null) {
                CanAcceptCard(card);
            }
        } else {
            dropZoneImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isDraggingOverZone = false;
        UpdateVisualFeedback(false);
    }

    public void ResetVisualFeedback() {
        isDraggingOverZone = false;
        if (dropZoneImage != null) {
            dropZoneImage.color = defaultColor;
        }
    }
}