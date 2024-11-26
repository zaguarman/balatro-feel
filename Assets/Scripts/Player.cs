using UnityEngine.Events;
using System;
using System.Collections.Generic;

[Serializable] public class PlayerDamagedUnityEvent : UnityEvent<int> { }

public interface IPlayer : ITarget {
    int Health { get; set; }
    bool IsPlayer1();
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
    public PlayerDamagedUnityEvent OnDamaged { get; } = new PlayerDamagedUnityEvent();

    private readonly GameMediator gameMediator;

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
        Hand = new List<ICard>();
        Battlefield = new List<ICreature>();
        gameMediator = GameMediator.Instance;
    }

    public bool IsPlayer1() {
        var gameManager = GameManager.Instance;
        return gameManager != null && gameManager.Player1 == this;
    }

    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
        OnDamaged.Invoke(amount);
        // Event notification is handled through OnDamaged event listener in GameMediator
    }

    public void AddToHand(ICard card) {
        if (card == null) return;
        Hand.Add(card);
        gameMediator.NotifyGameStateChanged();
    }

    public void AddToBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Add(creature);
        gameMediator.NotifyGameStateChanged();
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Remove(creature);
        gameMediator.NotifyGameStateChanged();
    }

    public bool IsValidTarget(IPlayer controller) => true;
}