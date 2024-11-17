using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    private GameUI gameUI;

    // Event for UI updates
    public event Action OnGameStateChanged;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializeGame();
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        gameUI = GetComponent<GameUI>();
        if (gameUI == null) {
            Debug.LogError("GameUI component not found!");
        }
    }

    private void InitializeGame() {
        GameContext = new GameContext();

        // Initialize players
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;
    }

    public void PlayCard(CardData cardData, Player player) {
        Card card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    public void AttackWithCreature(Creature attacker, Player attackingPlayer, Creature target = null) {
        if (target == null) {
            // Attack player directly
            attackingPlayer.Opponent.TakeDamage(attacker.Attack);
        } else {
            // Attack creature
            target.TakeDamage(attacker.Attack, GameContext);
            attacker.TakeDamage(target.Attack, GameContext);

            // Clean up dead creatures
            CleanupDeadCreatures(Player1);
            CleanupDeadCreatures(Player2);
        }

        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    private void CleanupDeadCreatures(Player player) {
        player.Battlefield.RemoveAll(creature => creature.Health <= 0);
    }

    private void NotifyGameStateChanged() {
        OnGameStateChanged?.Invoke();
    }

    public void RestartGame() {
        InitializeGame();
        NotifyGameStateChanged();
    }

    // Optional: Method to check game end
    public bool CheckGameEnd() {
        if (Player1.Health <= 0) {
            Debug.Log("Player 2 Wins!");
            return true;
        }

        if (Player2.Health <= 0) {
            Debug.Log("Player 1 Wins!");
            return true;
        }

        return false;
    }
}