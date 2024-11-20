using System.Collections.Generic;

public interface IPlayer {
    int Health { get; set; }
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<ICreature> Battlefield { get; }
    string Id { get; }

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
    public string Id { get; private set; }

    public Player() {
        Id = System.Guid.NewGuid().ToString();
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if (Health < 0) Health = 0;
    }

    public void AddToHand(ICard card) {
        Hand.Add(card);
    }

    public void AddToBattlefield(ICreature creature) {
        Battlefield.Add(creature);
    }

    public void RemoveFromBattlefield(ICreature creature) {
        Battlefield.Remove(creature);
    }
}