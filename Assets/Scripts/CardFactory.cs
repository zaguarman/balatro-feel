using static Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public static class CardFactory {
    private static Dictionary<string, CardData> cardDataCache = new Dictionary<string, CardData>();

    // Create actual card instance from CardData
    public static ICard CreateCard(CardData cardData) {
        if (cardData == null) return null;

        ICard card = null;
        switch (cardData) {
            case CreatureData creatureData:
                card = new Creature(creatureData.cardName,
                    creatureData.attack, creatureData.health);
                foreach (var effect in creatureData.effects) {
                    card.Effects.Add(effect);
                }
                break;
        }
        return card;
    }

    // Create CardController with proper setup
    public static CardController CreateCardController(ICard cardData, IPlayer owner, Transform parent, GameReferences gameReferences) {
        var cardPrefab = gameReferences.GetCardPrefab();
        if (cardPrefab == null) return null;

        var cardObj = GameObject.Instantiate(cardPrefab, parent);
        var controller = cardObj.GetComponent<CardController>();
        if (controller != null) {
            var data = CreateCardDataFromCard(cardData);
            controller.Setup(data, owner);
        }
        return controller;
    }

    // Create CardData from ICard
    private static CardData CreateCardDataFromCard(ICard card) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = card.Name;

        if (card is ICreature creature) {
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        }

        return cardData;
    }

    // Get or create CardData template
    public static CardData GetOrCreateCardData(string name, Action<CardData> setup) {
        if (cardDataCache.TryGetValue(name, out var existingData)) {
            return existingData;
        }

        var newData = ScriptableObject.CreateInstance<CreatureData>();
        setup(newData);
        cardDataCache[name] = newData;
        return newData;
    }

    // Helper method for setting up common card event handlers
    public static void SetupCardEventHandlers(
        CardController controller,
        UnityAction<CardController> onBeginDrag = null,
        UnityAction<CardController> onEndDrag = null,
        UnityAction<CardController> onDrop = null,
        System.Action onPointerEnter = null,
        System.Action onPointerExit = null) {
        if (controller == null) return;

        // Clean up any existing listeners first
        CleanupCardEventHandlers(controller);

        // Add new listeners
        if (onBeginDrag != null)
            controller.OnBeginDragEvent.AddListener(onBeginDrag);

        if (onEndDrag != null)
            controller.OnEndDragEvent.AddListener(onEndDrag);

        if (onDrop != null)
            controller.OnCardDropped.AddListener(onDrop);

        if (onPointerEnter != null)
            controller.OnPointerEnterHandler += onPointerEnter;

        if (onPointerExit != null)
            controller.OnPointerExitHandler += onPointerExit;
    }

    public static void CleanupCardEventHandlers(CardController controller) {
        if (controller == null) return;

        // Remove all listeners from Unity Events
        controller.OnBeginDragEvent.RemoveAllListeners();
        controller.OnEndDragEvent.RemoveAllListeners();
        controller.OnCardDropped.RemoveAllListeners();

        // Clear C# event handlers
        controller.OnPointerEnterHandler = null;
        controller.OnPointerExitHandler = null;
    }
}