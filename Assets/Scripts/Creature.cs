public interface ICreature : ICard, ITarget {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage, GameContext context);
    bool IsValidTarget(IPlayer controller) => true;
}

public class Creature : ICreature {
    public string Name { get; private set; }
    public string TargetId { get; private set; }
    public string CardId { get; private set; }
    public int Attack { get; private set; }
    public int Health { get; private set; }

    public Creature(string name, int attack, int health) {
        Name = name;
        Attack = attack;
        Health = health;
        CardId = System.Guid.NewGuid().ToString();
        TargetId = System.Guid.NewGuid().ToString();
    }

    public void Play(GameContext context, IPlayer owner) {
        context.AddAction(new SummonCreatureAction(this, owner));
    }

    public void TakeDamage(int damage, GameContext context) {
        Health -= damage;
    }

    public bool IsValidTarget(IPlayer controller) {
        throw new System.NotImplementedException();
    }
}