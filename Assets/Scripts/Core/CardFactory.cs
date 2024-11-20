using UnityEngine;
using System.Collections.Generic;

public static class CardFactory {
    public static Card CreateCard(CardData cardData) {
        Card baseCard = CreateBaseCard(cardData);
        return WrapWithEffects(baseCard, cardData.effects);
    }

    private static Card CreateBaseCard(CardData cardData) {
        switch (cardData) {
            case CreatureData creatureData:
                return new Creature(creatureData.cardName, creatureData.attack, creatureData.health);
            default:
                Debug.LogError($"Unsupported card type: {cardData.GetType()}");
                return null;
        }
    }

    private static Card WrapWithEffects(Card card, List<CardEffect> effects) {
        Card wrappedCard = card;

        foreach (var effect in effects) {
            switch (effect.effectType) {
                case EffectType.Triggered:
                    wrappedCard = new TriggeredEffectDecorator(wrappedCard, effect.trigger, effect.actions);
                    break;

                case EffectType.Continuous:
                    wrappedCard = new ContinuousEffectDecorator(wrappedCard, effect.actions);
                    break;

                case EffectType.Immediate:
                    // Immediate effects could be handled directly in the Play method
                    break;
            }
        }

        return wrappedCard;
    }
}