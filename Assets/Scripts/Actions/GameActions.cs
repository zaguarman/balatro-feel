using static Enums;
using static DebugLogger;
using UnityEngine;

public interface IGameAction { void Execute(); }

public class SummonCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly IPlayer owner;
    private readonly ITarget target;

    public SummonCreatureAction(ICreature creature, IPlayer owner, ITarget target = null) {
        this.creature = creature;
        this.owner = owner;
        this.target = target;
        Log($"Created SummonCreatureAction for {creature.Name} targeting slot {target.TargetId} with {creature.Effects.Count} effects", LogTag.Actions | LogTag.Creatures);
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

        owner.AddToBattlefield(creature, target);
    }

    public override string ToString() {
        return $"SummonCreatureAction: Creature={creature?.Name}, Player={(owner.IsPlayer1() ? "1" : "2")}, Target={target?.TargetId}";
    }
}

public class DamagePlayerAction : IGameAction {
    private IPlayer target;
    private int damage;
    public IPlayer GetTargetPlayer() => target;
    public int GetDamage() => damage;

    public DamagePlayerAction(IPlayer target, int damage) {
        this.target = target;
        this.damage = damage;
        Log($"Created - Target: {target}, Damage: {damage}", LogTag.Actions | LogTag.Players | LogTag.Combat);
    }

    public void Execute() {
        target.TakeDamage(damage);
        Log($"{damage} damage dealt to {target}", LogTag.Actions | LogTag.Players | LogTag.Combat);
    }

    public override string ToString() {
        return $"DamagePlayerAction: Target Player={(target.IsPlayer1() ? "1" : "2")}, Damage={damage}";
    }
}

public class DamageCreatureAction : IGameAction {
    private readonly ICreature target;
    private readonly int damage;
    private readonly ICreature attacker;
    private readonly bool isDirectDamage;
    public ICreature GetTarget() => target;
    public ICreature GetAttacker() => attacker;
    public int GetDamage() => damage;

    public DamageCreatureAction(ICreature target, int damage, ICreature attacker = null, bool isDirectDamage = false) {
        this.target = target;
        this.damage = damage;
        this.attacker = attacker;
        this.isDirectDamage = isDirectDamage;
        Log($"Created DamageAction - Target: {target?.Name}, Damage: {damage}, Attacker: {attacker?.Name}, DirectDamage: {isDirectDamage}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);
    }

    public void Execute() {
        if (target == null) return;

        var weatherSystem = GameManager.Instance?.WeatherSystem;
        float modifier = weatherSystem?.GetDamageModifier(isDirectDamage) ?? 0f;
        int modifiedDamage = damage;

        // Apply weather modifiers
        if (modifier != 0f) {
            modifiedDamage = Mathf.Max(0, modifiedDamage + Mathf.RoundToInt(modifier));
            Log($"Weather modified damage from {damage} to {modifiedDamage} (modifier: {modifier})",
                LogTag.Actions | LogTag.Combat | LogTag.Effects);
        }

        Log($"Executing DamageAction - Target: {target.Name}, Original Damage: {damage}, Modified Damage: {modifiedDamage}, Weather Modifier: {modifier}, Attacker: {attacker?.Name}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);

        if (target is Creature creatureTarget) {
            creatureTarget.TakeDamage(modifiedDamage, attacker);
        } else {
            target.TakeDamage(modifiedDamage);
        }
    }

    public override string ToString() {
        return $"DamageCreatureAction: Target={target?.Name}, Damage={damage}, Attacker={attacker?.Name}, DirectDamage={isDirectDamage}";
    }
}

public class DirectDamageAction : IGameAction {
    private readonly ICreature target;
    private readonly int damage;
    private readonly ICreature source;
    public ICreature GetTarget() => target;
    public ICreature GetSource() => source;
    public int GetDamage() => damage;

    public DirectDamageAction(ICreature target, int damage, ICreature source = null) {
        this.target = target;
        this.damage = damage;
        this.source = source;
        Log($"Created DirectDamageAction - Source: {source?.Name}, Target: {target?.Name}, Damage: {damage}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);
    }

    public void Execute() {
        if (target == null) return;

        var weatherSystem = GameManager.Instance?.WeatherSystem;
        float modifier = weatherSystem?.GetDamageModifier(true) ?? 0f; // true for direct damage
        int modifiedDamage = damage;

        // Apply weather modifiers
        if (modifier != 0f) {
            modifiedDamage = Mathf.Max(0, modifiedDamage + Mathf.RoundToInt(modifier));
            Log($"Weather modified direct damage from {damage} to {modifiedDamage} (modifier: {modifier})",
                LogTag.Actions | LogTag.Combat | LogTag.Effects);
        }

        Log($"Executing DirectDamageAction - Source: {source?.Name}, Target: {target.Name}, Original Damage: {damage}, Modified Damage: {modifiedDamage}, Weather Modifier: {modifier}",
            LogTag.Actions | LogTag.Creatures | LogTag.Combat);

        if (target is Creature creatureTarget) {
            creatureTarget.TakeDamage(modifiedDamage, source);
        } else {
            target.TakeDamage(modifiedDamage);
        }
    }

    public override string ToString() {
        return $"DirectDamageAction: Source={source?.Name}, Target={target?.Name}, Damage={damage}";
    }
}
public class SwapCreaturesAction : IGameAction {
    private readonly ICreature fromCreature;
    private readonly ICreature toCreature;
    private readonly ITarget fromSlot;
    private readonly ITarget toSlot;
    private readonly IPlayer owner;
    // Added methods to help with arrow creation
    public ICreature GetCreature1() => fromCreature;
    public ICreature GetCreature2() => toCreature;


    public SwapCreaturesAction(ICreature fromCreature, ICreature toCreature, ITarget fromSlot, ITarget toSlot, IPlayer owner) {
        this.fromCreature = fromCreature;
        this.toCreature = toCreature;
        this.fromSlot = fromSlot;
        this.toSlot = toSlot;
        this.owner = owner;
        Log($"Created SwapCreaturesAction between {fromCreature.Name} (slot {fromSlot}) and {toCreature.Name} (slot {toSlot})",
            LogTag.Actions | LogTag.Creatures);
    }

    public void Execute() {
        if (fromCreature == null || toCreature == null || owner == null) {
            LogError("Cannot execute swap - one or more components are null", LogTag.Actions | LogTag.Creatures);
            return;
        }

        owner.RemoveFromBattlefield(fromCreature);
        owner.RemoveFromBattlefield(toCreature);

        owner.AddToBattlefield(fromCreature, toSlot);
        owner.AddToBattlefield(toCreature, fromSlot);

        Log($"Executed swap between {fromCreature.Name} and {toCreature.Name}", LogTag.Actions | LogTag.Creatures);
    }

    public override string ToString() {
        return $"SwapCreaturesAction: From={fromCreature?.Name} (Slot={fromSlot}), To={toCreature?.Name} (Slot={toSlot})";
    }
}

public class PlayCardAction : IGameAction {
    private readonly ICard card;
    private readonly IPlayer owner;
    private readonly ITarget target;

    public PlayCardAction(ICard card, IPlayer owner, ITarget target) {
        this.card = card;
        this.owner = owner;
        this.target = target;
        Log($"Created PlayCardAction for {card.Name} targeting slot {target}", LogTag.Actions | LogTag.Cards);
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
            var summonAction = new SummonCreatureAction(creature, owner, target);
            GameManager.Instance.ActionsQueue.AddAction(summonAction);
        }

        // Process any immediate effects
        card.Play(owner, GameManager.Instance.ActionsQueue, target);

        Log($"Executed PlayCardAction for {card.Name}", LogTag.Actions | LogTag.Cards);
    }

    public override string ToString() {
        return $"PlayCardAction: Card={card?.Name}, Owner={owner?.Name}, Target={target?.TargetId}";
    }
}

public class MarkCombatTargetAction : IGameAction {
    private readonly ICreature attacker;
    private readonly BattlefieldSlot targetSlot;
    public ICreature GetAttacker() => attacker;
    public BattlefieldSlot GetTargetSlot() => targetSlot;

    public MarkCombatTargetAction(ICreature attacker, ITarget targetSlot) {
        this.attacker = attacker;
        this.targetSlot = (BattlefieldSlot)targetSlot;
    }

    public void Execute() {
        if (targetSlot.IsOccupied()) {
            var targetCreature = targetSlot.OccupyingCreature;
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

    public override string ToString() {
        return $"MarkCombatTargetAction: Attacker={attacker?.Name}, TargetSlot={targetSlot.name}";
    }
}

public class MoveCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly ITarget fromSlot;
    private readonly ITarget toSlot;
    private readonly IPlayer player;
    // Getter methods for arrow visualization
    public ICreature GetCreature() => creature;
    public ITarget GetFromSlot() => fromSlot;
    public ITarget GetToSlot() => toSlot;

    public MoveCreatureAction(ICreature creature, ITarget fromSlot, ITarget toSlot, IPlayer player) {
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

        // Check if target slot is occupied
        var targetCreature = ((BattlefieldSlot)toSlot).OccupyingCreature;
        if (targetCreature != null) {
            // Handle swap
            HandleSwap(targetCreature);
        } else {
            // Handle simple move
            HandleMove();
        }

        Log($"Executed move action for {creature.Name} from slot {fromSlot} to slot {toSlot}",
            LogTag.Actions | LogTag.Creatures);
    }

    private void HandleSwap(ICreature targetCreature) {
        // Temporarily remove both creatures from battlefield
        player.RemoveFromBattlefield(creature);
        player.RemoveFromBattlefield(targetCreature);

        // Re-add them in swapped order
        if (player is Player p) {
            p.AddToBattlefield(creature, toSlot);
            p.AddToBattlefield(targetCreature, fromSlot);
        }
    }

    private void HandleMove() {
        player.RemoveFromBattlefield(creature);
        if (player is Player p) {
            p.AddToBattlefield(creature, toSlot);
        }
    }

    public override string ToString() {
        return $"MoveCreatureAction: Creature={creature?.Name}, FromSlot={fromSlot?.TargetId}, ToSlot={toSlot?.TargetId}";
    }
}