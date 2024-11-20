using System.Collections.Generic;
using UnityEngine;

public static class CardFactory {
    public static ICard CreateCard(CardData cardData) {
        ICard baseCard = CreateBaseCard(cardData);
        return WrapWithEffects(baseCard, cardData.effects);
    }

    private static ICard CreateBaseCard(CardData cardData) {
        switch (cardData) {
            case CreatureData creatureData:
                return new Creature(creatureData.cardName, creatureData.attack, creatureData.health);
            default:
                Debug.LogError($"Unsupported card type: {cardData.GetType()}");
                return null;
        }
    }

    private static ICard WrapWithEffects(ICard card, List<CardEffect> effects) {
        ICard wrappedCard = card;

        foreach (var effect in effects) {
            switch (effect.effectType) {
                case EffectType.Triggered:
                    wrappedCard = new TriggeredEffectDecorator(wrappedCard, effect.trigger, effect.actions);
                    break;
                case EffectType.Continuous:
                    wrappedCard = new ContinuousEffectDecorator(wrappedCard, effect.actions);
                    break;
            }
        }

        return wrappedCard;
    }
}