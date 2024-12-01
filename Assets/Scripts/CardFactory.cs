using static Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class CardFactory {
    public static ICard CreateCard(CardData cardData) {
        ICard card = null;

        switch (cardData) {
            case CreatureData creatureData:
                var creature = new Creature(creatureData.cardName, creatureData.attack, creatureData.health);
                foreach (var effect in creatureData.effects) {
                    creature.Effects.Add(effect);
                }
                card = creature;
                break;
            default:
                Debug.LogError($"Unsupported card type: {cardData.GetType()}");
                break;
        }

        return card;
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