using static Enums;
using System.Collections.Generic;
using UnityEngine;

public interface ICard {
    string Name { get; }
    string CardId { get; }
    List<CardEffect> Effects { get; }
    void Play(GameContext context, IPlayer owner);
}

public class Card : ICard {
    public string Name { get; protected set; }
    public string CardId { get; protected set; }
    public List<CardEffect> Effects { get; protected set; }

    public Card(string name) {
        Name = name;
        CardId = System.Guid.NewGuid().ToString();
        Effects = new List<CardEffect>();
    }

    public virtual void Play(GameContext context, IPlayer owner) {
        // Handle OnPlay effects
        foreach (var effect in Effects) {
            if (effect.trigger == EffectTrigger.OnPlay) {
                HandleEffect(effect, context, owner);
            }
        }
    }

    protected void HandleEffect(CardEffect effect, GameContext context, IPlayer owner) {
        Debug.Log($"Handling effect for {Name}");
        foreach (var action in effect.actions) {
            ExecuteAction(action, context, owner);
        }
    }

    private void ExecuteAction(EffectAction action, GameContext context, IPlayer owner) {
        var validTargets = context.TargetingSystem.GetValidTargets(owner, action.targetType);
        Debug.Log($"Found {validTargets.Count} valid targets for {action.actionType}");

        foreach (var target in validTargets) {
            switch (action.actionType) {
                case ActionType.Damage:
                    if (target is IPlayer playerTarget) {
                        context.AddAction(new DamagePlayerAction(playerTarget, action.value));
                    } else if (target is ICreature creatureTarget) {
                        context.AddAction(new DamageCreatureAction(creatureTarget, action.value));
                    }
                    break;
                case ActionType.Heal:
                    // Implement healing logic
                    break;
                case ActionType.Draw:
                    if (target is IPlayer playerToDraw) {
                        // Implement draw logic
                    }
                    break;
            }
        }
    }
}