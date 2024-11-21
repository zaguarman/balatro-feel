using System;
using System.Collections.Generic;

public interface IPlayer : ITarget {
    int Health { get; set; }
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<ICreature> Battlefield { get; }
    void TakeDamage(int amount);
    void AddToHand(ICard card);
    void AddToBattlefield(ICreature creature);
    void RemoveFromBattlefield(ICreature creature);
    event Action<int> OnDamaged;
}

public class Player : IPlayer {
    public int Health { get; set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; } = new List<ICard>();
    public List<ICreature> Battlefield { get; private set; } = new List<ICreature>();
    public string TargetId { get; private set; }
    public event Action<int> OnDamaged;

    private GameMediator mediator;

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
        mediator = GameMediator.Instance;
        mediator.RegisterPlayer(this);
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if (Health < 0) Health = 0;
        OnDamaged?.Invoke(amount);
        mediator.NotifyPlayerDamaged(this, amount);
    }

    public void AddToBattlefield(ICreature creature) {
        Battlefield.Add(creature);
        mediator.NotifyGameStateChanged();
    }

    public void RemoveFromBattlefield(ICreature creature) {
        Battlefield.Remove(creature);
        mediator.NotifyGameStateChanged();
    }

    public void AddToHand(ICard card) {
        Hand.Add(card);
        mediator.NotifyGameStateChanged();
    }

    public bool IsValidTarget(IPlayer controller) => true;
}
