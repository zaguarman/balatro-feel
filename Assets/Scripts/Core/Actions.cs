public interface IAction {
    void Execute(IGameContext context);
}

public class AttackAction : IAction {
    public ICreature Attacker { get; }
    public ITarget Target { get; }
    public int Amount => Attacker.Attack;

    public AttackAction(ICreature attacker, ITarget target) {
        Attacker = attacker;
        Target = target;
    }

    public void Execute(IGameContext context) {
        if (Target is ICreature creature) {
            creature.TakeDamage(Amount, context);
        } else if (Target is IPlayer player) {
            player.TakeDamage(Amount);
        }
    }
}

public class DamageAction : IAction {
    private readonly int amount;
    private readonly ITarget target;

    public DamageAction(int amount, ITarget target) {
        this.amount = amount;
        this.target = target;
    }

    public void Execute(IGameContext context) {
        if (target is IPlayer player) {
            player.TakeDamage(amount);
        } else if (target is ICreature creature) {
            creature.Health -= amount;
        }
    }
}

public class BuffAction : IAction {
    public ICreature Target { get; }
    public (int Attack, int Health) Values { get; }

    public BuffAction(ICreature target, int attackBuff, int healthBuff) {
        Target = target;
        Values = (attackBuff, healthBuff);
    }

    public void Execute(IGameContext context) {
        Target.ReceiveAction(this);
    }
}