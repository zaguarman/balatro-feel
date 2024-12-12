using static DebugLogger;
using static Enums;

public interface ICreature : ICard {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage);
}

public class Creature : Card, ICreature {
    public int Attack { get; private set; }
    public int Health { get; private set; }
    private bool isDead = false;
    private IPlayer owner;

    public Creature(string name, int attack, int health, IPlayer owner = null) : base(name) {
        Attack = attack;
        Health = health;
        this.owner = owner;
    }

    public void SetOwner(IPlayer newOwner) {
        this.owner = newOwner;
    }

    public override void Play(IPlayer owner, ActionsQueue context) {
        Log($"Playing {Name} with {Effects.Count} effects", LogTag.Creatures | LogTag.Cards | LogTag.Actions);
        this.owner = owner;
        var summonAction = new SummonCreatureAction(this, owner);
        context.AddAction(summonAction);
        Log($"Added SummonCreatureAction to queue for {Name}", LogTag.Creatures | LogTag.Actions);
    }

    public void TakeDamage(int damage) {
        if (isDead) return;

        Health = System.Math.Max(0, Health - damage);
        Log($"{Name} took {damage} damage, health now: {Health}. Has {Effects.Count} effects", LogTag.Creatures | LogTag.Combat);

        var gameManager = GameManager.Instance;
        if (gameManager != null && owner != null) {
            Log($"Processing OnDamage effects for {Name}", LogTag.Creatures | LogTag.Effects);
            HandleEffect(EffectTrigger.OnDamage, gameManager.ActionsQueue);
        } else {
            LogError($"Cannot process damage effects - GameManager: {gameManager != null}, Owner: {owner != null}", LogTag.Creatures | LogTag.Effects);
        }

        var gameMediator = GameMediator.Instance;
        if (gameMediator != null) {
            gameMediator.NotifyCreatureDamaged(this, damage);

            if (Health <= 0 && !isDead) {
                isDead = true;
                if (owner != null) {
                    owner.RemoveFromBattlefield(this);
                }
                gameMediator.NotifyCreatureDied(this);
            }
        }
    }

    public void HandleEffect(EffectTrigger trigger, ActionsQueue actionsQueue) {
        Log($"Handling {trigger} effect for {Name} with {Effects.Count} effects", LogTag.Creatures | LogTag.Effects);
        foreach (var effect in Effects) {
            Log($"Checking effect - Trigger: {effect.trigger}, Actions: {effect.actions.Count}", LogTag.Creatures | LogTag.Effects);
            if (effect.trigger == trigger) {
                foreach (var action in effect.actions) {
                    Log($"Processing action - Type: {action.actionType}, Value: {action.value}, Target: {action.targetType}", LogTag.Creatures | LogTag.Actions);
                    switch (action.actionType) {
                        case ActionType.Damage:
                            HandleDamageEffect(action, actionsQueue);
                            break;
                    }
                }
            }
        }
    }

    private void HandleDamageEffect(EffectAction action, ActionsQueue actionsQueue) {
        if (owner == null) {
            LogError($"Cannot handle damage effect for {Name} - owner is null", LogTag.Creatures | LogTag.Effects);
            return;
        }

        Log($"Getting targets for {Name}'s damage effect. TargetType: {action.targetType}, Damage: {action.value}", LogTag.Creatures | LogTag.Actions);
        var targets = TargetingSystem.GetValidTargets(owner, action.targetType);
        Log($"Found {targets.Count} targets for {Name}'s damage effect", LogTag.Creatures | LogTag.Actions);

        foreach (var target in targets) {
            if (target is ICreature creature) {
                Log($"Adding DamageCreatureAction - Source: {Name}, Target: {creature.Name}, Damage: {action.value}", LogTag.Creatures | LogTag.Actions | LogTag.Combat);
                actionsQueue.AddAction(new DamageCreatureAction(creature, action.value, this));
            } else if (target is IPlayer player) {
                Log($"Adding DamagePlayerAction - Target: Player {(player.IsPlayer1() ? "1" : "2")}, Damage: {action.value}", LogTag.Creatures | LogTag.Actions | LogTag.Players | LogTag.Combat);
                actionsQueue.AddAction(new DamagePlayerAction(player, action.value));
            }
        }
    }
}