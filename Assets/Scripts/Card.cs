using System.Collections.Generic;

public interface ICard : IEntity {
    List<CardEffect> Effects { get; }
    void Play(IPlayer owner, ActionsContext context);
}

public class Card : Entity, ICard {
    public List<CardEffect> Effects { get; protected set; }

    public Card(string name) : base(name) {
        Effects = new List<CardEffect>();
    }

    public virtual void Play(IPlayer owner, ActionsContext context) {
        // Base implementation does nothing
    }
}