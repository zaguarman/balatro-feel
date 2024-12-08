// File: BattlefieldUI.cs
using System.Collections.Generic;
using System.Linq;

public class BattlefieldUI : CardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();

    protected override void HandleCardDropped(CardController card) {
        if (card != null && CanAcceptCard(card)) {
            var gameManager = GameManager.Instance;
            if (gameManager != null && card.GetCardData() is CreatureData creatureData) {
                gameManager.PlayCard(creatureData, card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2);
                gameMediator?.NotifyGameStateChanged();
            }
        }
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

        foreach (var card in cards.ToList()) {
            RemoveCard(card);
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        cards.Clear();
        creatureCards.Clear();

        foreach (var creature in player.Battlefield) {
            var controller = CreateCreatureCard(creature);
            if (controller != null) {
                creatureCards[creature.TargetId] = controller;
                AddCard(controller);
            }
        }

        dropZoneHandler?.ResetVisualFeedback();
    }

    // Override card interaction methods to disable dragging for battlefield cards
    protected override void OnCardBeginDrag(CardController card) { }
    protected override void OnCardEndDrag(CardController card) { }
    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}