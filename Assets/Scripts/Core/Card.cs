public abstract class Card {
    public string Name { get; protected set; }
    public virtual void Play(GameContext context, Player owner) { }
}

