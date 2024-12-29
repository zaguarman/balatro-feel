using static DebugLogger;
using static Enums;
using System.Linq;

public interface ICreature : ICard {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage);
    IPlayer Owner { get; }
    string Id { get; }
    void SetOwner(IPlayer owner);
}

public class Creature : Card, ICreature {
    public int Attack { get; private set; }
    public int Health { get; private set; }
    private bool isDead = false;
    public IPlayer Owner { get; private set; }
    public string Id { get; private set; }

    private ICreature lastAttacker;

    public Creature(string name, int attack, int health) : base(name) {
        Attack = attack;
        Health = health;
        Id = TargetId;
    }

    public void SetOwner(IPlayer owner) {
        Owner = owner;
    }

    public override void Play(IPlayer owner, ActionsQueue context, ITarget target = null) {
        Log($"Playing {Name} with {Effects.Count} effects", LogTag.Creatures | LogTag.Cards | LogTag.Actions);
        Owner = owner;
        context.AddAction(new SummonCreatureAction(this, owner, target));
    }

    public void TakeDamage(int damage) {
        TakeDamage(damage, null);
    }

    internal void TakeDamage(int damage, ICreature attacker) {
        if (isDead) return;

        lastAttacker = attacker;
        Health = System.Math.Max(0, Health - damage);
        Log($"{Name} took {damage} damage, health now: {Health}. Has {Effects.Count} effects",
            LogTag.Creatures | LogTag.Combat);

        var gameManager = GameManager.Instance;
        if (gameManager?.ActionsQueue != null) {
            Log($"Processing OnDamage effects for {Name}", LogTag.Creatures | LogTag.Effects);
            HandleEffect(EffectTrigger.OnDamage, gameManager.ActionsQueue);
        }

        if (Health <= 0 && !isDead) {
            isDead = true;
            Owner?.RemoveFromBattlefield(this);
            Log($"Creature died: {Name}", LogTag.Creatures);
            GameMediator.Instance?.NotifyCreatureDied(this);
        }

        GameMediator.Instance?.NotifyCreatureDamaged(this, damage);
        lastAttacker = null;
    }

    public void HandleEffect(EffectTrigger trigger, ActionsQueue actionsQueue) {
        // Skip if this effect was already handled in current resolution chain
        if (actionsQueue.IsEffectProcessed(TargetId, trigger)) {
            Log($"Skipping already processed {trigger} effect for {Name}", LogTag.Effects);
            return;
        }

        Log($"Handling {trigger} effect for {Name} with {Effects.Count} effects", LogTag.Creatures | LogTag.Effects);

        foreach (var effect in Effects.Where(e => e.trigger == trigger)) {
            Log($"Processing effect - Trigger: {effect.trigger}, Actions: {effect.actions.Count}",
                LogTag.Creatures | LogTag.Effects);

            foreach (var action in effect.actions) {
                Log($"Processing action - Type: {action.actionType}, Value: {action.value}, Target: {action.targetType}",
                    LogTag.Creatures | LogTag.Actions);

                if (action.actionType == ActionType.Damage) {
                    ProcessDamageEffect(action, actionsQueue);
                }
            }
        }

        actionsQueue.MarkEffectProcessed(TargetId, trigger);
        Log($"Marked {trigger} effect as processed for {Name}", LogTag.Effects);
    }

    private void ProcessDamageEffect(EffectAction action, ActionsQueue actionsQueue) {
        if (Owner == null) {
            LogError($"Cannot handle damage effect for {Name} - Owner is null", LogTag.Creatures | LogTag.Effects);
            return;
        }

        Log($"Processing damage effect for {Name}. TargetType: {action.targetType}, Damage: {action.value}",
            LogTag.Creatures | LogTag.Actions);

        // Handle retaliatory damage
        if (lastAttacker != null && Effects.Any(e => e.trigger == EffectTrigger.OnDamage)) {
            Log($"Targeting attacker {lastAttacker.Name} for retaliation damage",
                LogTag.Creatures | LogTag.Actions);
            actionsQueue.AddAction(new DirectDamageAction(lastAttacker, action.value, this));
            return;
        }

        // Handle normal targeting
        var targets = TargetingSystem.GetValidTargets(Owner, action.targetType);
        Log($"Found {targets.Count} targets for {Name}'s damage effect",
            LogTag.Creatures | LogTag.Actions);

        foreach (var target in targets) {
            if (target is ICreature creature) {
                Log($"Adding DirectDamageAction - Source: {Name}, Target: {creature.Name}, Damage: {action.value}",
                    LogTag.Creatures | LogTag.Actions | LogTag.Combat);
                actionsQueue.AddAction(new DirectDamageAction(creature, action.value, this));
            } else if (target is IPlayer player) {
                Log($"Adding DamagePlayerAction - Target: Player {(player.IsPlayer1() ? "1" : "2")}, Damage: {action.value}",
                    LogTag.Creatures | LogTag.Actions | LogTag.Players | LogTag.Combat);
                actionsQueue.AddAction(new DamagePlayerAction(player, action.value));
            }
        }
    }
}