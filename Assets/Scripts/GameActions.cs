using static Enums;

public interface IGameAction { void Execute(); }

public class SummonCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly IPlayer owner;

    public SummonCreatureAction(ICreature creature, IPlayer owner) {
        this.creature = creature;
        this.owner = owner;
        DebugLogger.Log($"Created for {creature.Name} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Creatures);
    }

    public void Execute() {
        DebugLogger.Log($"Executing for {creature.Name}", LogTag.Actions | LogTag.Creatures);

        if (creature == null || owner == null) {
            DebugLogger.LogError("Creature or owner is null", LogTag.Actions | LogTag.Creatures);
            return;
        }

        if (creature is Creature c) {
            c.SetOwner(owner);
        }

        var actionsQueue = GameManager.Instance?.ActionsQueue;
        if (creature is Creature c2 && actionsQueue != null) {
            DebugLogger.Log($"Processing OnPlay effects for {creature.Name} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Effects);
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
        DebugLogger.Log($"Created - Target: {target}, Damage: {damage}", LogTag.Actions | LogTag.Players | LogTag.Combat);
    }

    public void Execute() {
        target.TakeDamage(damage);
        DebugLogger.Log($"{damage} damage dealt to {target}", LogTag.Actions | LogTag.Players | LogTag.Combat);
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
        DebugLogger.Log($"Created - Target: {target?.Name} (TargetId: {target?.TargetId}), " +
                        $"Damage: {damage}, Attacker: {attacker?.Name} (TargetId: {attacker?.TargetId})",
                        LogTag.Actions | LogTag.Creatures | LogTag.Combat);
    }

    public void Execute() {
        DebugLogger.Log($"Executing - Target: {target?.Name} (TargetId: {target?.TargetId}), " +
                        $"Damage: {damage}, Attacker: {attacker?.Name} (TargetId: {attacker?.TargetId})",
                        LogTag.Actions | LogTag.Creatures | LogTag.Combat);
        target?.TakeDamage(damage);
    }

    public ICreature GetTarget() {
        DebugLogger.Log($"Getting target: {target?.Name} (TargetId: {target?.TargetId})", LogTag.Actions | LogTag.Creatures);
        return target;
    }

    public ICreature GetAttacker() {
        DebugLogger.Log($"Getting attacker: {attacker?.Name} (TargetId: {attacker?.TargetId})", LogTag.Actions | LogTag.Creatures);
        return attacker;
    }

    public int GetDamage() => damage;
}
