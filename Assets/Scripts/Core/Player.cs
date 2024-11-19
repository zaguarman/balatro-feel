using System;
using System.Collections.Generic;

public interface IPlayer : ITarget {
    int Health { get; set; }
    IPlayer Opponent { get; set; }
    List<ICreature> Battlefield { get; }
    void TakeDamage(int amount);
    void AddToBattlefield(ICreature creature);
}

public class Player : IPlayer {
    public int Health { get; set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICreature> Battlefield { get; } = new List<ICreature>();
    public string Id { get; } = System.Guid.NewGuid().ToString();

    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
    }

    public void AddToBattlefield(ICreature creature) {
        Battlefield.Add(creature);
    }

    public void ReceiveAction(IAction action) {
        // Handle action reception - e.g. trigger effects
    }
}
