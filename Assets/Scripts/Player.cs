using UnityEngine.Events;
using System;
using System.Collections.Generic;

[Serializable] public class PlayerDamagedUnityEvent : UnityEvent<int> { }

public interface IPlayer : ITarget {
    int Health { get; set; }
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<ICreature> Battlefield { get; }
    void TakeDamage(int amount);
    void AddToHand(ICard card);
    void AddToBattlefield(ICreature creature);
    void RemoveFromBattlefield(ICreature creature);
    PlayerDamagedUnityEvent OnDamaged { get; }
}

public class Player : IPlayer {
    public int Health { get; set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; }
    public List<ICreature> Battlefield { get; private set; }
    public string TargetId { get; private set; }
    public PlayerDamagedUnityEvent OnDamaged { get; private set; }

    private readonly GameMediator mediator;

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
        Hand = new List<ICard>();
        Battlefield = new List<ICreature>();
        OnDamaged = new PlayerDamagedUnityEvent();
        mediator = GameMediator.Instance;
        mediator?.RegisterPlayer(this);
    }

    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
        OnDamaged?.Invoke(amount);
        mediator?.NotifyPlayerDamaged(this, amount);
    }

    public void AddToBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Add(creature);
        mediator?.NotifyGameStateChanged();
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Remove(creature);
        mediator?.NotifyGameStateChanged();
    }

    public void AddToHand(ICard card) {
        if (card == null) return;
        Hand.Add(card);
        mediator?.NotifyGameStateChanged();
    }

    public bool IsValidTarget(IPlayer controller) => true;
}