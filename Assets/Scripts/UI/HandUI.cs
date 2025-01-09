using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DebugLogger;

public class HandUI : CardContainer {
    public override void Initialize(IPlayer player) {
        base.Initialize(player);
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddHandStateChangedListener(UpdateUI);
            gameMediator.AddGameInitializedListener(OnGameInitialized);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveHandStateChangedListener(UpdateUI);
            gameMediator.RemoveGameInitializedListener(OnGameInitialized);
        }
    }

    private void OnGameInitialized() {
        UpdateUI(Player);
    }

    public override void UpdateUI(IPlayer player) {
        if (!IsInitialized || Player == null) return;

        if (player != Player) return;

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

    protected override void SetupCardEventHandlers(CardController controller) {
        controller.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        controller.OnEndDragEvent.AddListener(OnCardEndDrag);
        controller.OnCardDropped.AddListener(OnCardDropped);
    }

    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;
        card.transform.SetAsLastSibling();
        Log($"Begin dragging card from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
    }

    protected override void OnCardEndDrag(CardController card) {
        Log($"End dragging card from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
        UpdateLayout();
    }

    protected override void OnCardDropped(CardController card) {
        Log($"Card dropped from hand: {card.GetCardData()?.cardName}", LogTag.UI | LogTag.Cards);
        UpdateLayout();
    }

    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}