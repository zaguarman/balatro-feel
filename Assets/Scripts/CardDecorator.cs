using System.Collections.Generic;
using UnityEngine;

public interface ICardDecorator : ICard {
    ICard BaseCard { get; }
}

// Triggered Effect Decorator
public class TriggeredEffectDecorator : ICardDecorator {
    public ICard BaseCard { get; }
    public string Name => BaseCard.Name;
    public string CardId => BaseCard.CardId;
    public EffectTrigger Trigger { get; }
    public List<EffectAction> Actions { get; }

    public TriggeredEffectDecorator(ICard baseCard, EffectTrigger trigger, List<EffectAction> actions) {
        BaseCard = baseCard;
        Trigger = trigger;
        Actions = actions;
        Debug.Log($"Created TriggeredEffectDecorator for {baseCard.Name} with trigger {trigger}");
    }

    public void Play(GameContext context, IPlayer owner) {
        BaseCard.Play(context, owner);
        if (Trigger == EffectTrigger.OnPlay) {
            Debug.Log($"Triggering OnPlay effect for {Name}");
            HandleTrigger(context, owner);
        }
    }

    public void HandleTrigger(GameContext context, IPlayer owner) {
        Debug.Log($"Handling trigger for {Name}");
        foreach (var action in Actions) {
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
                        Debug.Log($"Adding {action.value} damage action to player {playerTarget.TargetId}");
                    } else if (target is ICreature creatureTarget) {
                        context.AddAction(new DamageCreatureAction(creatureTarget, action.value));
                        Debug.Log($"Adding {action.value} damage action to creature {creatureTarget.Name}");
                    }
                    break;
            }
        }
    }
}

// Continuous Effect Decorator
public class ContinuousEffectDecorator : ICardDecorator {
    public ICard BaseCard { get; }
    public string Name => BaseCard.Name;
    public string CardId => BaseCard.CardId;

    public List<EffectAction> Actions { get; internal set; }

    private bool applied;

    public ContinuousEffectDecorator(ICard baseCard, List<EffectAction> actions) {
        BaseCard = baseCard;
        this.Actions = actions;
        this.applied = false;
    }

    public void Play(GameContext context, IPlayer owner) {
        BaseCard.Play(context, owner);
        if (!applied) {
            foreach (var action in Actions) {
                ApplyEffect(action, context, owner);
            }
            applied = true;
        }
    }

    private void ApplyEffect(EffectAction action, GameContext context, IPlayer owner) {
        // Implement continuous effects
    }
}
