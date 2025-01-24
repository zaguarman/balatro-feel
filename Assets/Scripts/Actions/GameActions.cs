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
        // 1. Set owner first
        creature.SetOwner(owner);

        // 2. Add to battlefield before processing effects
        owner.AddToBattlefield(creature, target);

        // 3. Process effects after battlefield placement
        if (creature is Creature c) {
            Log($"Processing OnPlay effects for {c.Name} with {c.Effects.Count} effects",
                LogTag.Actions | LogTag.Effects);
            c.HandleEffect(EffectTrigger.OnPlay, GameManager.Instance.ActionsQueue);
        }
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

        var fromSlotComponent = (BattlefieldSlot)fromSlot;
        var toSlotComponent = (BattlefieldSlot)toSlot;

        // Get the current creature in the target slot
        var targetCreature = toSlotComponent.OccupyingCreature;

        if (targetCreature != null) {
            HandleSwap(fromSlotComponent, toSlotComponent);
        } else {
            HandleMove(fromSlotComponent, toSlotComponent);
        }

        Log($"Executed move action for {creature.Name} from slot {fromSlot} to slot {toSlot}",
            LogTag.Actions | LogTag.Creatures);
    }

    private void HandleSwap(BattlefieldSlot fromSlot, BattlefieldSlot toSlot) {
        // Get the CardControllers from both slots
        var fromController = fromSlot.OccupyingCard;
        var toController = toSlot.OccupyingCard;

        // Clear both slots without destroying the CardControllers
        fromSlot.ClearSlot(false); // Pass false to avoid destroying the controller
        toSlot.ClearSlot(false);

        // Assign controllers to the opposite slots
        fromSlot.AssignCreature(toController);
        toSlot.AssignCreature(fromController);

        // Update the Slot references on the creatures
        if (fromController != null && fromController.GetLinkedCreature() != null) {
            fromController.GetLinkedCreature().Slot = toSlot;
        }
        if (toController != null && toController.GetLinkedCreature() != null) {
            toController.GetLinkedCreature().Slot = fromSlot;
        }

        GameMediator.Instance?.NotifyBattlefieldStateChanged(player);
    }

    private void HandleMove(BattlefieldSlot fromSlot, BattlefieldSlot toSlot) {
        var fromController = fromSlot.OccupyingCard;

        // Clear the target slot if occupied (optional, based on game rules)
        if (toSlot.IsOccupied()) {
            toSlot.ClearSlot(true); // Destroy existing if necessary
        }

        // Move the controller to the new slot
        fromSlot.ClearSlot(false); // Don't destroy
        toSlot.AssignCreature(fromController);

        // Update the creature's Slot reference
        if (fromController != null && fromController.GetLinkedCreature() != null) {
            fromController.GetLinkedCreature().Slot = toSlot;
        }

        GameMediator.Instance?.NotifyBattlefieldStateChanged(player);
    }

    public override string ToString() {
        return $"MoveCreatureAction: Creature={creature?.Name}, FromSlot={fromSlot?.TargetId}, ToSlot={toSlot?.TargetId}";
    }
}