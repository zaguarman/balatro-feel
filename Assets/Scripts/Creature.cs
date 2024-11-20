public interface ICreature : ICard {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage, GameContext context);
}

public class Creature : ICreature {
    public string Name { get; private set; }
    public string Id { get; private set; }
    public int Attack { get; private set; }
    public int Health { get; private set; }

    public Creature(string name, int attack, int health) {
        Name = name;
        Attack = attack;
        Health = health;
        Id = System.Guid.NewGuid().ToString();
    }

    public void Play(GameContext context, IPlayer owner) {
        context.AddAction(new SummonCreatureAction(this, owner));
    }

    public void TakeDamage(int damage, GameContext context) {
        Health -= damage;
    }
}