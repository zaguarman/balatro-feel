using System.Collections.Generic;

public interface ICardDecorator : ICard {
    ICard BaseCard { get; }
}

// Triggered Effect Decorator
public class TriggeredEffectDecorator : ICardDecorator {
    public ICard BaseCard { get; }
    public string Name => BaseCard.Name;
    public string Id => BaseCard.Id;

    public EffectTrigger Trigger { get; internal set; }
    public List<EffectAction> Actions { get; internal set; }


    public TriggeredEffectDecorator(ICard baseCard, EffectTrigger trigger, List<EffectAction> actions) {
        BaseCard = baseCard;
        this.Trigger = trigger;
        this.Actions = actions;
    }

    public void Play(GameContext context, IPlayer owner) {
        BaseCard.Play(context, owner);
        if (Trigger == EffectTrigger.OnPlay) {
            HandleTrigger(context, owner);
        }
    }

    public void HandleTrigger(GameContext context, IPlayer owner) {
        foreach (var action in Actions) {
            ExecuteAction(action, context, owner);
        }
    }

    private void ExecuteAction(EffectAction action, GameContext context, IPlayer owner) {
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
        }
    }
}

// Continuous Effect Decorator
public class ContinuousEffectDecorator : ICardDecorator {
    public ICard BaseCard { get; }
    public string Name => BaseCard.Name;
    public string Id => BaseCard.Id;

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
