public interface IEffect {
    void Apply(IGameContext context, ITarget target);
    bool IsActive { get; }
    void Remove();
}

public interface IEffectStrategy {
    IEffect CreateEffect(IAction action, string sourceId = null);
}

public class ImmediateStrategy : IEffectStrategy {
    public IEffect CreateEffect(IAction action, string sourceId = null) =>
        new ImmediateEffect(action);
}

public class TriggeredStrategy : IEffectStrategy {
    private readonly ITrigger trigger;

    public TriggeredStrategy(ITrigger trigger) {
        this.trigger = trigger;
    }

    public IEffect CreateEffect(IAction action, string sourceId = null) =>
        new TriggeredEffect(action, trigger);
}

public class ContinuousStrategy : IEffectStrategy {
    public IEffect CreateEffect(IAction action, string sourceId = null) =>
        new ContinuousEffect(action, sourceId);
}

public class ImmediateEffect : IEffect {
    private readonly IAction action;
    public bool IsActive { get; private set; } = true;

    public ImmediateEffect(IAction action) {
        this.action = action;
    }

    public void Apply(IGameContext context, ITarget target) {
        if (IsActive) {
            action.Execute(context);
            Remove();
        }
    }

    public void Remove() => IsActive = false;
}

public class TriggeredEffect : IEffect {
    private readonly IAction action;
    private readonly ITrigger trigger;
    public bool IsActive { get; private set; } = true;

    public TriggeredEffect(IAction action, ITrigger trigger) {
        this.action = action;
        this.trigger = trigger;
    }

    public void Apply(IGameContext context, ITarget target) {
        if (IsActive && trigger.IsTriggered(context)) {
            action.Execute(context);
        }
    }

    public void Remove() => IsActive = false;
}

public class ContinuousEffect : IEffect {
    private readonly IAction action;
    private readonly string sourceId;
    public bool IsActive { get; private set; } = true;

    public ContinuousEffect(IAction action, string sourceId) {
        this.action = action;
        this.sourceId = sourceId;
    }

    public void Apply(IGameContext context, ITarget target) {
        if (IsActive && target is ICreature creature && creature.Id != sourceId) {
            action.Execute(context);
        }
    }

    public void Remove() => IsActive = false;
}
