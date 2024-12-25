using static Enums;
using static DebugLogger;

public interface IGameAction { void Execute(); }

public class SummonCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly IPlayer owner;
    private readonly int targetSlot;

    public SummonCreatureAction(ICreature creature, IPlayer owner, int targetSlot = -1) {
        this.creature = creature;
        this.owner = owner;
        this.targetSlot = targetSlot;
        Log($"Created SummonCreatureAction for {creature.Name} targeting slot {targetSlot} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Creatures);
    }

    public void Execute() {
        Log($"Executing SummonCreatureAction for {creature.Name}", LogTag.Actions | LogTag.Creatures);

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

        // Add to battlefield with specific slot if provided
        if (targetSlot >= 0) {
            owner.AddToBattlefield(creature, targetSlot);
        } else {
            owner.AddToBattlefield(creature);
        }
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

    public IPlayer GetTargetPlayer() => target;
    public int GetDamage() => damage;
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

public class SwapCreaturesAction : IGameAction {
    private readonly ICreature creature1;
    private readonly ICreature creature2;
    private readonly int slot1Index;
    private readonly int slot2Index;
    private readonly IPlayer owner;

    public SwapCreaturesAction(ICreature creature1, ICreature creature2, int slot1Index, int slot2Index, IPlayer owner) {
        this.creature1 = creature1;
        this.creature2 = creature2;
        this.slot1Index = slot1Index;
        this.slot2Index = slot2Index;
        this.owner = owner;
        Log($"Created SwapCreaturesAction between {creature1.Name} (slot {slot1Index}) and {creature2.Name} (slot {slot2Index})",
            LogTag.Actions | LogTag.Creatures);
    }

    public void Execute() {
        if (creature1 == null || creature2 == null || owner == null) {
            LogError("Cannot execute swap - one or more components are null", LogTag.Actions | LogTag.Creatures);
            return;
        }

        // Temporarily remove both creatures from battlefield
        owner.RemoveFromBattlefield(creature1);
        owner.RemoveFromBattlefield(creature2);

        // Re-add them in swapped order using the slot-specific overload
        ((Player)owner).AddToBattlefield(creature1, slot2Index);
        ((Player)owner).AddToBattlefield(creature2, slot1Index);

        Log($"Executed swap between {creature1.Name} and {creature2.Name}", LogTag.Actions | LogTag.Creatures);
    }

    // Added methods to help with arrow creation
    public ICreature GetCreature1() => creature1;
    public ICreature GetCreature2() => creature2;
}

public class PlayCardAction : IGameAction {
    private readonly ICard card;
    private readonly IPlayer owner;
    private readonly int targetSlot;

    public PlayCardAction(ICard card, IPlayer owner, int targetSlot = -1) {
        this.card = card;
        this.owner = owner;
        this.targetSlot = targetSlot;
        Log($"Created PlayCardAction for {card.Name} targeting slot {targetSlot}", LogTag.Actions | LogTag.Cards);
    }

    public void Execute() {
        if (card == null || owner == null) {
            LogError("Cannot execute PlayCardAction - card or owner is null", LogTag.Actions);
            return;
        }

        // Remove the card from hand first
        owner.Hand.Remove(card);

        // Play the card with the specific slot target
        if (card is ICreature creature) {
            var summonAction = new SummonCreatureAction(creature, owner, targetSlot);
            GameManager.Instance.ActionsQueue.AddAction(summonAction);
        }

        // Process any immediate effects
        card.Play(owner, GameManager.Instance.ActionsQueue);

        Log($"Executed PlayCardAction for {card.Name}", LogTag.Actions | LogTag.Cards);
    }
}

public class MarkCombatTargetAction : IGameAction {
    private readonly ICreature attacker;
    private readonly BattlefieldSlot targetSlot;

    public MarkCombatTargetAction(ICreature attacker, BattlefieldSlot targetSlot) {
        this.attacker = attacker;
        this.targetSlot = targetSlot;
    }

    public void Execute() {
        if (targetSlot.IsOccupied) {
            var targetCreature = targetSlot.OccupyingCard?.GetLinkedCreature();
            if (targetCreature != null) {
                var damageAction = new DamageCreatureAction(targetCreature, attacker.Attack, attacker);
                GameManager.Instance.ActionsQueue.AddAction(damageAction);
            }
        } else {
            var targetPlayer = attacker.Owner?.Opponent;
            if (targetPlayer != null) {
                GameManager.Instance.ActionsQueue.AddAction(
                    new DamagePlayerAction(targetPlayer, attacker.Attack)
                );
            }
        }
    }

    public ICreature GetAttacker() => attacker;
    public BattlefieldSlot GetTargetSlot() => targetSlot;
}

public class MoveCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly int fromSlot;
    private readonly int toSlot;
    private readonly IPlayer player;

    public MoveCreatureAction(ICreature creature, int fromSlot, int toSlot, IPlayer player) {
        this.creature = creature;
        this.fromSlot = fromSlot;
        this.toSlot = toSlot;
        this.player = player;
    }

    public void Execute() {
        if (creature == null || player == null) {
            LogError("Invalid move action - creature or player is null", LogTag.Actions);
            return;
        }

        // Move the creature to the new slot
        player.AddToBattlefield(creature, toSlot);
        Log($"Executed move action for {creature.Name} from slot {fromSlot} to slot {toSlot}",
            LogTag.Actions | LogTag.Creatures);
    }

    public ICreature GetCreature() => creature;
    public int GetFromSlot() => fromSlot;
    public int GetToSlot() => toSlot;
}