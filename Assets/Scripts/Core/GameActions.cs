using UnityEngine;

public class SummonCreatureAction : IGameAction {
    private readonly ICreature creature;
    private readonly IPlayer owner;

    public SummonCreatureAction(ICreature creature, IPlayer owner) {
        this.creature = creature;
        this.owner = owner;
    }

    public void Execute(GameContext context) {
        owner.AddToBattlefield(creature);
        GameEventMediator.Instance.NotifyCreatureSummoned(creature as Creature, owner as Player);
        Debug.Log($"{creature.Name} of {owner} summoned to battlefield");
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