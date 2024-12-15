using static Enums;
using static DebugLogger;

public interface IGameAction { void Execute(); }

public class SummonCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly IPlayer owner;

    public SummonCreatureAction(ICreature creature, IPlayer owner) {
        this.creature = creature;
        this.owner = owner;
        Log($"Created for {creature.Name} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Creatures);
    }

    public void Execute() {
        Log($"Executing for {creature.Name}", LogTag.Actions | LogTag.Creatures);

        if (creature == null || owner == null) {
            LogError("Creature or owner is null", LogTag.Actions | LogTag.Creatures);
            return;
        }

        if (creature is Creature c) {
            c.SetOwner(owner);
        }

        var actionsQueue = GameManager.Instance?.ActionsQueue;
        if (creature is Creature c2 && actionsQueue != null) {
            Log($"Processing OnPlay effects for {creature.Name} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Effects);
            c2.HandleEffect(EffectTrigger.OnPlay, actionsQueue);
        }

        owner.AddToBattlefield(creature);
    }
}

public class DamagePlayerAction : IGameAction {
    private IPlayer target;
    private int damage;

    public DamagePlayerAction(IPlayer target, int damage) {
        this.target = target;
        this.damage = damage;
        Log($"Created - Target: {target}, Damage: {damage}", LogTag.Actions | LogTag.Players | LogTag.Combat);
    }

    public void Execute() {
        target.TakeDamage(damage);
        Log($"{damage} damage dealt to {target}", LogTag.Actions | LogTag.Players | LogTag.Combat);
    }
}

public class DamageCreatureAction : IGameAction {
    private readonly ICreature target;
    private readonly int damage;
    private readonly ICreature attacker;

    public DamageCreatureAction(ICreature target, int damage, ICreature attacker = null) {
        this.target = target;
        this.damage = damage;
        this.attacker = attacker;
        Log($"Created DamageAction - Target: {target?.Name}, Damage: {damage}, Attacker: {attacker?.Name}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);
    }

    public void Execute() {
        if (target == null) return;

        Log($"Executing DamageAction - Target: {target.Name}, Damage: {damage}, Attacker: {attacker?.Name}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);

        if (target is Creature creatureTarget) {
            creatureTarget.TakeDamage(damage, attacker);
        } else {
            target.TakeDamage(damage);
        }
    }

    public ICreature GetTarget() => target;
    public ICreature GetAttacker() => attacker;
    public int GetDamage() => damage;
}

public class DirectDamageAction : IGameAction {
    private readonly ICreature target;
    private readonly int damage;
    private readonly ICreature source;

    public DirectDamageAction(ICreature target, int damage, ICreature source = null) {
        this.target = target;
        this.damage = damage;
        this.source = source;
        Log($"Created DirectDamageAction - Source: {source?.Name}, Target: {target?.Name}, Damage: {damage}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);
    }

    public void Execute() {
        if (target == null) return;

        Log($"Executing DirectDamageAction - Source: {source?.Name}, Target: {target.Name}, Damage: {damage}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);

        if (target is Creature creatureTarget) {
            creatureTarget.TakeDamage(damage, source);
        } else {
            target.TakeDamage(damage);
        }
    }

    public ICreature GetTarget() => target;
    public ICreature GetSource() => source;
    public int GetDamage() => damage;
}