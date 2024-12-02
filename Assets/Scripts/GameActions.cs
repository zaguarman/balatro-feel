using UnityEngine;

public interface IGameAction {
    void Execute();
}

public class SummonCreatureAction : IGameAction {
    private readonly ICard card;
    private readonly IPlayer owner;
    private readonly GameMediator gameMediator;

    public SummonCreatureAction(ICard card, IPlayer owner) {
        this.card = card;
        this.owner = owner;
        this.gameMediator = GameMediator.Instance;
    }

    public void Execute() {
        if (card is ICreature creature) {
            gameMediator.NotifyCreaturePreSummon(creature);
            owner.AddToBattlefield(creature);
            gameMediator.NotifyCreatureSummoned(creature, owner);
        }
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

    public void Execute() {
        target.TakeDamage(damage);
        Debug.Log($"{damage} damage dealt to {target}");
    }
}

public class DamageCreatureAction : IGameAction {
    private ICreature target;
    private int damage;

    public DamageCreatureAction(ICreature target, int damage) {
        this.target = target;
        this.damage = damage;
        Debug.Log($"Created DamageCreatureAction: {damage} damage to {target.Name}");
    }

    public void Execute() {
        target.TakeDamage(damage);
        Debug.Log($"Executed DamageCreatureAction: {damage} damage to {target.Name}");
    }
}