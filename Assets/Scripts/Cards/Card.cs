using System.Collections.Generic;
using static DebugLogger;

public interface ICard : IEntity {
    List<CardEffect> Effects { get; }
    void Play(IPlayer owner, ActionsQueue context, ITarget target = null);
}

public class Card : Entity, ICard {
    public List<CardEffect> Effects { get; protected set; }

    public Card(string name) : base(name) {
        Effects = new List<CardEffect>();
    }

    public virtual void Play(IPlayer owner, ActionsQueue context, ITarget target = null) {
        Log($"[Card] Playing {Name} with {Effects.Count} effects and target {target.TargetId}", LogTag.Cards | LogTag.Actions);
        // Base implementation for non-creature cards
        foreach (var effect in Effects) {
            Log($"[Card] Processing effect with trigger {effect.trigger}", LogTag.Cards | LogTag.Actions | LogTag.Effects);
        }
    }
}