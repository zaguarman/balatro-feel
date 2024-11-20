using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    public event Action OnGameStateChanged;
    public event Action<IPlayer> OnPlayerDamaged;
    public event Action<ICreature> OnCreatureDamaged;
    public event Action<IPlayer> OnGameOver;

    public void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializeGame();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeGame() {
        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;
        NotifyGameStateChanged();
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        NotifyGameStateChanged();
    }

    public void AttackWithCreature(ICreature attacker, IPlayer attackingPlayer, ICreature target = null) {
        if (target == null) {
            attackingPlayer.Opponent.TakeDamage(attacker.Attack);
            OnPlayerDamaged?.Invoke(attackingPlayer.Opponent);
        } else {
            target.TakeDamage(attacker.Attack, GameContext);
            attacker.TakeDamage(target.Attack, GameContext);
            OnCreatureDamaged?.Invoke(target);
            OnCreatureDamaged?.Invoke(attacker);

            CleanupDeadCreatures(Player1);
            CleanupDeadCreatures(Player2);
        }

        NotifyGameStateChanged();
    }

    private void CleanupDeadCreatures(IPlayer player) {
        var deadCreatures = player.Battlefield.FindAll(creature => creature.Health <= 0);
        foreach (var creature in deadCreatures) {
            player.RemoveFromBattlefield(creature);
        }
    }

    public void NotifyGameStateChanged() {
        OnGameStateChanged?.Invoke();
    }
}