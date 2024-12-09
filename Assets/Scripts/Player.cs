using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerDamagedUnityEvent : UnityEvent<int> { }

public interface IPlayer : IEntity, IDamageable {
    bool IsPlayer1();
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<ICreature> Battlefield { get; }
    void AddToHand(ICard card);
    void AddToBattlefield(ICreature creature);
    void RemoveFromBattlefield(ICreature creature);
    PlayerDamagedUnityEvent OnDamaged { get; }
}

public class Player : Entity, IPlayer {
    public int Health { get; private set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; }
    public List<ICreature> Battlefield { get; private set; }
    public PlayerDamagedUnityEvent OnDamaged { get; } = new PlayerDamagedUnityEvent();

    private readonly GameMediator gameMediator;

    public Player(string name = "Player") : base(name) {
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
    }

    public void AddToHand(ICard card) {
        if (card == null) return;
        Hand.Add(card);
        gameMediator?.NotifyGameStateChanged();
    }

    public void AddToBattlefield(ICreature creature) {
        if (creature == null) return;
        Battlefield.Add(creature);
        //Debug.Log($"[Player] Added creature to battlefield: {creature.Name}");
        gameMediator?.NotifyGameStateChanged();
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;

        if (Battlefield.Remove(creature)) {
            Debug.Log($"[Player] Removed creature from battlefield: {creature.Name}");
            gameMediator?.NotifyGameStateChanged();
        }
    }
}