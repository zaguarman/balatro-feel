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

    protected override CardController CreateCard(ICard cardData) {
        var cardPrefab = gameReferences.GetCardPrefab();
        if (cardPrefab == null || cardData == null) return null;

        var cardObj = Instantiate(cardPrefab, transform);
        var controller = cardObj.GetComponent<CardController>();
        if (controller != null) {
            var data = CreateCardData(cardData);
            Log($"Creating card {cardData.Name} with {cardData.Effects.Count} effects", LogTag.UI | LogTag.Cards);
            controller.Setup(data, Player);
            SetupCardEventHandlers(controller);
        }
        return controller;
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

    private CardData CreateCardData(ICard card) {
        if (card is ICreature creature) {
            var creatureData = ScriptableObject.CreateInstance<CreatureData>();
            creatureData.cardName = creature.Name;
            creatureData.attack = creature.Attack;
            creatureData.health = creature.Health;

            creatureData.effects = new List<CardEffect>();
            foreach (var effect in creature.Effects) {
                var newEffect = new CardEffect {
                    effectType = effect.effectType,
                    trigger = effect.trigger,
                    actions = effect.actions.Select(a => new EffectAction {
                        actionType = a.actionType,
                        value = a.value,
                        targetType = a.targetType
                    }).ToList()
                };
                creatureData.effects.Add(newEffect);
            }

            Log($"Created CreatureData for {creature.Name} with {creatureData.effects.Count} effects",
                LogTag.UI | LogTag.Cards | LogTag.Effects);
            return creatureData;
        }
        return null;
    }

    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}