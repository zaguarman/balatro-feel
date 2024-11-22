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
    private readonly GameEvents gameEvents;
    public PlayerDamagedUnityEvent OnDamaged { get; } = new PlayerDamagedUnityEvent();

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
        Hand = new List<ICard>();
        Battlefield = new List<ICreature>();
        gameEvents = GameEvents.Instance;
    }

    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
        OnDamaged.Invoke(amount);
        gameEvents.NotifyPlayerDamaged(this, amount);
    }

    public void AddToBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Add(creature);
        gameEvents.NotifyGameStateChanged();
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Remove(creature);
        gameEvents.NotifyGameStateChanged();
    }

    public void AddToHand(ICard card) {
        if (card == null) return;
        Hand.Add(card);
        gameEvents.NotifyGameStateChanged();
    }

    public bool IsValidTarget(IPlayer controller) => true;
}