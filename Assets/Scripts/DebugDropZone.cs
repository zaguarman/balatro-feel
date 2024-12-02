using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class DebugDropZone : CardDropZone, IPointerEnterHandler, IPointerExitHandler {
    [Header("Visual Settings")]
    [SerializeField] private Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color validDropColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidDropColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private float visualFeedbackDuration = 0.5f;

    private Image dropZoneImage;
    private Color originalColor;
    private float visualFeedbackTimer;
    private bool isDraggingOverZone;

    protected override void Awake() {
        base.Awake();
        SetupVisuals();
    }

    private void SetupVisuals() {
        dropZoneImage = GetComponent<Image>();
        if (dropZoneImage == null) {
            dropZoneImage = gameObject.AddComponent<Image>();
        }

        // Make the image transparent but interactive
        dropZoneImage.color = defaultColor;
        originalColor = defaultColor;
    }

    public override bool CanAcceptCard(CardController card) {
        bool canAccept = base.CanAcceptCard(card);
        UpdateVisualFeedback(card, canAccept);
        return canAccept;
    }

    public override void OnCardDropped(CardController card) {
        if (!CanAcceptCard(card)) {
            ShowInvalidDropFeedback();
            Debug.Log("Card dropped but not accepted in drop zone.");
            return;
        }

        var cardData = card.GetCardData();
        if (cardData != null) {
            var gameManager = GameManager.Instance;
            if (gameManager != null) {
                gameManager.PlayCard(cardData, card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2);
                ShowValidDropFeedback();
            } else {
                Debug.LogError("GameManager not found when attempting to play card");
                ShowInvalidDropFeedback();
            }
        } else {
            Debug.LogError("Card dropped but CardData is null");
            ShowInvalidDropFeedback();
        }

        base.OnCardDropped(card);
    }

    private void UpdateVisualFeedback(CardController card, bool isValid) {
        if (isDraggingOverZone && dropZoneImage != null) {
            dropZoneImage.color = isValid ? validDropColor : invalidDropColor;
        }
    }

    private void ShowValidDropFeedback() {
        if (dropZoneImage != null) {
            dropZoneImage.color = validDropColor;
            visualFeedbackTimer = visualFeedbackDuration;
        }
    }

    private void ShowInvalidDropFeedback() {
        if (dropZoneImage != null) {
            dropZoneImage.color = invalidDropColor;
            visualFeedbackTimer = visualFeedbackDuration;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isDraggingOverZone = true;
        if (eventData.pointerDrag != null) {
            var card = eventData.pointerDrag.GetComponent<CardController>();
            if (card != null) {
                UpdateVisualFeedback(card, CanAcceptCard(card));
            }
        } else {
            dropZoneImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isDraggingOverZone = false;
        if (visualFeedbackTimer <= 0) {
            dropZoneImage.color = originalColor;
        }
    }

    private void Update() {
        // Handle the temporary visual feedback
        if (visualFeedbackTimer > 0) {
            visualFeedbackTimer -= Time.deltaTime;
            if (visualFeedbackTimer <= 0) {
                dropZoneImage.color = originalColor;
            }
        }
    }
}