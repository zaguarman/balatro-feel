using System.Collections.Generic;

public class Player {
    public int Health { get; set; } = 20;
    public Player Opponent { get; set; }
    public List<BalatroCard> Hand { get; private set; } = new List<BalatroCard>();
    public List<Creature> Battlefield { get; private set; } = new List<Creature>();
    public string Id { get; private set; }

    public Player() {
        Id = System.Guid.NewGuid().ToString();
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if (Health < 0) Health = 0;
    }

    public void AddToHand(BalatroCard card) {
        Hand.Add(card);
    }

    public void AddToBattlefield(Creature creature) {
        Battlefield.Add(creature);
    }
}