using System;

public interface ICard {
    string Name { get; }
    string CardId { get; }
    void Play(GameContext context, IPlayer owner);
}

public class CardManager {
    public ICard RemoveDecorator<T>(ICard card) where T : ICardDecorator {
        if (card is not ICardDecorator decorator) return card;
        if (decorator is T) return decorator.BaseCard;
        var baseCard = RemoveDecorator<T>(decorator.BaseCard);
        return CreateNewDecorator(decorator, baseCard);
    }

    private ICard CreateNewDecorator(ICardDecorator original, ICard newBase) {
        return original switch {
            TriggeredEffectDecorator triggered => new TriggeredEffectDecorator(newBase, triggered.Trigger, triggered.Actions),
            ContinuousEffectDecorator continuous => new ContinuousEffectDecorator(newBase, continuous.Actions),
            _ => throw new ArgumentException($"Unknown decorator type: {original.GetType()}")
        };
    }

    public bool HasDecorator<T>(ICard card) where T : ICardDecorator {
        var current = card;
        while (current is ICardDecorator decorator) {
            if (decorator is T) return true;
            current = decorator.BaseCard;
        }
        return false;
    }
}