using static Enums;

public interface ICreature : ICard, ITarget {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage, GameContext context);
}

public class Creature : Card, ICreature {
    public int Attack { get; private set; }
    public int Health { get; private set; }
    public string TargetId { get; private set; }

    public Creature(string name, int attack, int health) : base(name) {
        Attack = attack;
        Health = health;
        TargetId = System.Guid.NewGuid().ToString();
    }

    public override void Play(GameContext context, IPlayer owner) {
        base.Play(context, owner);
        context.AddAction(new SummonCreatureAction(this, owner));
    }

    public void TakeDamage(int damage, GameContext context) {
        Health -= damage;

        // Handle OnDamage effects
        foreach (var effect in Effects) {
            if (effect.trigger == EffectTrigger.OnDamage) {
                HandleEffect(effect, context, context.GetOwner(this));
            }
        }
    }

    public bool IsValidTarget(IPlayer controller) {
        // Implement targeting logic
        return true;
    }
}