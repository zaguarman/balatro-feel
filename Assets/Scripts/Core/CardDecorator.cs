using System.Collections.Generic;

public abstract class CardDecorator : Card {
    protected Card card;

    public CardDecorator(Card card) {
        this.card = card;
        this.Name = card.Name;
    }

    public override void Play(GameContext context, Player owner) {
        card.Play(context, owner);
    }
}

// Triggered Effect Decorator
public class TriggeredEffectDecorator : CardDecorator {
    private EffectTrigger trigger;
    private List<EffectAction> actions;

    public TriggeredEffectDecorator(Card card, EffectTrigger trigger, List<EffectAction> actions) : base(card) {
        this.trigger = trigger;
        this.actions = actions;
    }

    public void HandleTrigger(EffectTrigger triggerType, GameContext context, Player owner) {
        if (trigger == triggerType) {
            foreach (var action in actions) {
                ExecuteAction(action, context, owner);
            }
        }
    }

    private void ExecuteAction(EffectAction action, GameContext context, Player owner) {
        switch (action.actionType) {
            case ActionType.Damage:
                switch (action.targetType) {
                    case TargetType.Enemy:
                        context.AddAction(new DamagePlayerAction(owner.Opponent, action.value));
                        break;
                    case TargetType.Player:
                        context.AddAction(new DamagePlayerAction(owner, action.value));
                        break;
                }
                break;
                // Add other action types as needed
        }
    }
}

// Continuous Effect Decorator
public class ContinuousEffectDecorator : CardDecorator {
    private List<EffectAction> actions;
    private bool applied;

    public ContinuousEffectDecorator(Card card, List<EffectAction> actions) : base(card) {
        this.actions = actions;
        this.applied = false;
    }

    public override void Play(GameContext context, Player owner) {
        base.Play(context, owner);
        if (!applied) {
            foreach (var action in actions) {
                ApplyEffect(action, context, owner);
            }
            applied = true;
        }
    }

    private void ApplyEffect(EffectAction action, GameContext context, Player owner) {
        // Implement continuous effects like buffs, auras, etc.
    }
}
