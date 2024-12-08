using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldUI : BaseCardContainer, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();

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

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (!IsInitialized) return;

        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            RemoveCard(card);
            if (card != null) {
                Destroy(card.gameObject);
            }
            creatureCards.Remove(creature.TargetId);
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;

        // Clear existing cards
        foreach (var card in cards.ToList()) {
            RemoveCard(card);
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        cards.Clear();
        creatureCards.Clear();

        // Create new cards
        foreach (var creature in player.Battlefield) {
            var controller = CreateCard(creature);
            if (controller != null) {
                creatureCards[creature.TargetId] = controller;
                AddCard(controller);
            }
        }

        UpdateVisualFeedback(false);
    }

    protected virtual CardController CreateCard(ICreature creature) {
        var cardPrefab = gameReferences.GetCardPrefab();
        if (cardPrefab == null) return null;

        var cardObj = Instantiate(cardPrefab, transform);
        var controller = cardObj.GetComponent<CardController>();
        if (controller != null) {
            var data = CreateCardData(creature);
            controller.Setup(data, player);
        }
        return controller;
    }

    protected virtual CardData CreateCardData(ICreature creature) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = creature.Name;
        cardData.attack = creature.Attack;
        cardData.health = creature.Health;
        return cardData;
    }

    // Drop zone functionality
    public virtual bool CanAcceptCard(CardController card) {
        if (card == null) return false;
        bool canAccept = ValidateCardType(card) && (card.IsPlayer1Card() ? acceptPlayer1Cards : acceptPlayer2Cards);
        UpdateVisualFeedback(canAccept);
        return canAccept;
    }

    protected virtual bool ValidateCardType(CardController card) {
        return card != null && card.GetCardData() is CreatureData;
    }

    protected virtual void UpdateVisualFeedback(bool isValid) {
        if (dropZoneImage != null) {
            dropZoneImage.color = isDraggingOverZone ?
                (isValid ? validDropColor : invalidDropColor) :
                defaultColor;
        }
    }

    // Drop handlers
    public void OnDrop(PointerEventData eventData) {
        var card = eventData.pointerDrag?.GetComponent<CardController>();
        if (card != null && CanAcceptCard(card)) {
            var gameManager = GameManager.Instance;
            if (gameManager != null && card.GetCardData() is CreatureData creatureData) {
                gameManager.PlayCard(creatureData, card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2);
                gameMediator?.NotifyGameStateChanged();
            }
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

    // Override card interaction methods
    protected override void OnCardBeginDrag(CardController card) {
        // Battlefield cards don't support dragging
    }

    protected override void OnCardEndDrag(CardController card) {
        // Battlefield cards don't support dragging
    }

    protected override void OnCardDropped(CardController card) {
        // Handled through drop zone functionality
    }

    protected override void OnCardHoverEnter(CardController card) {
        // Show targeting UI or creature details
    }

    protected override void OnCardHoverExit(CardController card) {
        // Hide targeting UI or creature details
    }
}