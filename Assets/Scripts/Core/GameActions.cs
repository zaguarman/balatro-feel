public interface IGameAction {
    void Execute(GameContext context);
}

public class SummonCreatureAction : IGameAction {
    private Creature creature;
    private Player owner;

    public SummonCreatureAction(Creature creature, Player owner) {
        this.creature = creature;
        this.owner = owner;
    }

    public void Execute(GameContext context) {
        owner.AddToBattlefield(creature);
    }
}

public class DamagePlayerAction : IGameAction {
    private Player target;
    private int damage;

    public DamagePlayerAction(Player target, int damage) {
        this.target = target;
        this.damage = damage;
    }

    public void Execute(GameContext context) {
        target.TakeDamage(damage);
    }
}