using UnityEngine;

public interface IGameAction {
    void Execute(GameContext context);
}

public class SummonCreatureAction : IGameAction {
    private Creature creature;
    private IPlayer owner;

    public SummonCreatureAction(Creature creature, IPlayer owner) {
        this.creature = creature;
        this.owner = owner;
        Debug.Log($"SummonCreatureAction created - Creature: {creature}, Owner: {owner}");
    }

    public void Execute(GameContext context) {
        owner.AddToBattlefield(creature);
        Debug.Log($"{creature} added to battlefield");
    }
}

public class DamagePlayerAction : IGameAction {
    private IPlayer target;
    private int damage;

    public DamagePlayerAction(IPlayer target, int damage) {
        this.target = target;
        this.damage = damage;
        Debug.Log($"DamagePlayerAction created - Target: {target}, Damage: {damage}");
    }

    public void Execute(GameContext context) {
        target.TakeDamage(damage);
        Debug.Log($"{damage} damage dealt to {target}");
    }
}