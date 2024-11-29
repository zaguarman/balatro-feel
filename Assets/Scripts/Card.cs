public interface ICard {
    string Name { get; }
    string CardId { get; }
    void Play(GameContext context, IPlayer owner);
}