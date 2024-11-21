using System;
using System.Collections.Generic;
using UnityEngine;

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

    // Events
    public event Action<int> OnDamaged;

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
        // Register with GameMediator in Start instead of constructor
        if (GameMediator.Instance != null) {
            GameMediator.Instance.RegisterPlayer(this);
        }
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if (Health < 0) Health = 0;
        OnDamaged?.Invoke(amount);
        if (GameMediator.Instance != null) {
            GameMediator.Instance.NotifyPlayerDamaged(this, amount);
        }
    }

    public void AddToBattlefield(ICreature creature) {
        Battlefield.Add(creature);
        if (GameMediator.Instance != null) {
            GameMediator.Instance.NotifyGameStateChanged();
        }
    }

    public void RemoveFromBattlefield(ICreature creature) {
        Battlefield.Remove(creature);
        if (GameMediator.Instance != null) {
            GameMediator.Instance.NotifyGameStateChanged();
        }
    }

    public void AddToHand(ICard card) {
        Hand.Add(card);
        if (GameMediator.Instance != null) {
            GameMediator.Instance.NotifyGameStateChanged();
        }
    }

    public bool IsValidTarget(IPlayer controller) => true;
}