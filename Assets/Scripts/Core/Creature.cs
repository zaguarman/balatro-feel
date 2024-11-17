public class Creature : Card {
    public int Attack { get; protected set; }
    public int Health { get; protected set; }
    public string Id { get; private set; }

    public Creature(string name, int attack, int health) {
        Name = name;
        Attack = attack;
        Health = health;
        Id = System.Guid.NewGuid().ToString();
    }

    public override void Play(GameContext context, Player owner) {
        context.AddAction(new SummonCreatureAction(this, owner));
    }

    public virtual void TakeDamage(int damage, GameContext context) {
        Health -= damage;
        if (Health <= 0) {
            // Could add death handling here
        }
    }
}