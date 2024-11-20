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
}

public class Player : IPlayer {
    public int Health { get; set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; } = new List<ICard>();
    public List<ICreature> Battlefield { get; private set; } = new List<ICreature>();
    public string TargetId { get; private set; }

    public Player() {
        TargetId = System.Guid.NewGuid().ToString();
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if (Health < 0) Health = 0;
        Debug.Log($"Player {TargetId} took {amount} damage, health now: {Health}");
    }

    public void AddToHand(ICard card) {
        Hand.Add(card);
        Debug.Log($"Added card to hand: {card.Name}");
    }

    public void AddToBattlefield(ICreature creature) {
        Battlefield.Add(creature);
        Debug.Log($"Added creature to battlefield: {creature.Name}");
    }

    public void RemoveFromBattlefield(ICreature creature) {
        Battlefield.Remove(creature);
        Debug.Log($"Removed creature from battlefield: {creature.Name}");
    }

    public bool IsValidTarget(IPlayer controller) => true;
}