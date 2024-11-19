using System.Collections.Generic;

public interface ICreature : ITarget {
    string Name { get; }
    int Attack { get; set; }
    int Health { get; set; }
    bool IsAlive { get; }
    IList<IEffect> Effects { get; }
    void AddEffect(IEffect effect);
    void RemoveEffect(IEffect effect);
    void TakeDamage(int damage, IGameContext context);  
}

public class Creature : ICreature {

    private readonly List<IEffect> effects = new List<IEffect>();
    public string Name { get; }
    public string Id { get; }
    public int Attack { get; set; }
    public int Health { get; set; }
    public List<IEffect> Effects => effects;

    public bool IsAlive => Health > 0;

    IList<IEffect> ICreature.Effects => effects;

    public void TakeDamage(int damage, IGameContext context) {
        Health -= damage;
        if (!IsAlive) {
            context.AddAction(new CreatureDeathAction(this));
        }
    }

    public void AddEffect(IEffect effect) {
        effects.Add(effect);
    }

    public void RemoveEffect(IEffect effect) {
        effects.Remove(effect);
    }

    public Creature(string name, int attack, int health, params IEffect[] cardEffects) {
        Name = name;
        Id = System.Guid.NewGuid().ToString();
        Attack = attack;
        Health = health;
        effects.AddRange(cardEffects);
    }

    public void Play(IGameContext context, IPlayer owner) {
        context.AddAction(new SummonCreatureAction(this, owner));
        foreach (var effect in effects) {
            effect.Apply(context, this);
        }
    }

    public void ReceiveAction(IAction action) {
        switch (action) {
            case AttackAction attack:
                TakeDamage(attack.Amount, context);
                break;
            case BuffAction buff:
                var (attackBuff, healthBuff) = buff.Values;
                Attack += attackBuff;
                Health += healthBuff;
                break;
        }
    }
}