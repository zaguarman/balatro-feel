using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DebugLogger;

public class HandUI : CardContainer {
    private void Start() {
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            Initialize(player);
        } else {
            InitializationManager.Instance.OnSystemInitialized.AddListener(Initialize);
        }
    }

    private void InitializeReferences() {
        if (!InitializationManager.Instance.IsComponentInitialized<GameManager>()) {
            LogWarning("GameManager not initialized yet, will retry later", LogTag.UI | LogTag.Initialization);
            return;
        }

        if (player != null) {
            Initialize(player);
        }
    }

    public override void Initialize(IPlayer assignedPlayer) {
        player = assignedPlayer;
        base.Initialize(player);
    }

    public override void OnGameStateChanged() {
        UpdateUI();
    }

    public override void OnGameInitialized() {
        UpdateUI();
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;

        foreach (var card in cards.ToList()) {
            RemoveCard(card);
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        cards.Clear();

        foreach (var cardData in player.Hand) {
            var controller = CreateCard(cardData);
            if (controller != null) {
                AddCard(controller);
            }
        }

        UpdateLayout();
    }

    protected override CardController CreateCard(ICard cardData) {
        var cardPrefab = gameReferences.GetCardPrefab();
        if (cardPrefab == null || cardData == null) return null;

        var cardObj = Instantiate(cardPrefab, transform);
        var controller = cardObj.GetComponent<CardController>();
        if (controller != null) {
            var data = CreateCardData(cardData);
            Log($"Creating card {cardData.Name} with {cardData.Effects.Count} effects", LogTag.UI | LogTag.Cards);
            controller.Setup(data, player);
            SetupCardEventHandlers(controller);
        }
        return controller;
    }

    protected override void SetupCardEventHandlers(CardController card) {
        base.SetupCardEventHandlers(card);
        // Additional setup if needed
    }

    public override void OnCardBeginDrag(CardController card) {
        if (card == null) return;
        card.transform.SetAsLastSibling();
        Log($"Begin dragging card from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
    }

    public override void OnCardEndDrag(CardController card) {
        Log($"End dragging card from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
        UpdateLayout();
    }

    public override void OnCardDropped(CardController card) {
        Log($"Card dropped from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
        UpdateLayout();
    }

    protected override CardData CreateCardData(ICard card) {
        if (card is ICreature creature) {
            return base.CreateCardData(creature);
        }
        
        var cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardName = card.Name;
        return cardData;
    }

    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}