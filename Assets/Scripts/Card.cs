using static Enums;
using System.Collections.Generic;

public interface ICard {
    string Name { get; }
    string CardId { get; }
    List<CardEffect> Effects { get; }
    void Play(IPlayer owner);
}

public class Card : ICard {
    public string Name { get; protected set; }
    public string CardId { get; protected set; }
    public List<CardEffect> Effects { get; protected set; }

    public Card(string name) {
        Name = name;
        CardId = System.Guid.NewGuid().ToString();
        Effects = new List<CardEffect>();
    }

    public virtual void Play(IPlayer owner) {

    }
}