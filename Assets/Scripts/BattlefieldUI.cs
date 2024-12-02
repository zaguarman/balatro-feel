using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlefieldUI : BaseCardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();

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

    protected override void OnCardBeginDrag(CardController card) {
        // Battlefield cards typically don't support dragging
    }

    protected override void OnCardEndDrag(CardController card) {
        // Battlefield cards typically don't support dragging
    }

    protected override void OnCardDropped(CardController card) {
        // Battlefield cards handle drops through BattlefieldDropZone
    }

    protected override void OnCardHoverEnter(CardController card) {
        // Show targeting UI or creature details
    }

    protected override void OnCardHoverExit(CardController card) {
        // Hide targeting UI or creature details
    }
}