
public interface ICreature : ICard {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage);
}

public class Creature : Card, ICreature {
    public int Attack { get; private set; }
    public int Health { get; private set; }

    public Creature(string name, int attack, int health) : base(name) {
        Attack = attack;
        Health = health;
    }

    public override void Play(IPlayer owner) {
        owner.AddToBattlefield(this);
    }

    public void TakeDamage(int damage) {
        Health -= damage;
    }
}