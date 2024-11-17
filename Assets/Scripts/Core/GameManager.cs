using UnityEngine;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private DamageResolver damageResolutionController;

    // Events
    public event Action OnGameStateChanged;
    public event Action<Player> OnPlayerDamaged;
    public event Action<Creature> OnCreatureDamaged;
    public event Action<Player> OnGameOver;

    private bool isGameActive = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializeGame();
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (!gameUI) {
            gameUI = FindObjectOfType<GameUI>();
        }
        if (!damageResolutionController) {
            damageResolutionController = FindObjectOfType<DamageResolver>();
        }

        if (gameUI == null) {
            Debug.LogError("GameUI component not found!");
        }
        if (damageResolutionController == null) {
            Debug.LogError("DamageResolver not found!");
        }

        SetupEventListeners();
    }

    private void SetupEventListeners() {
        OnGameStateChanged += UpdateGameState;
        OnPlayerDamaged += HandlePlayerDamaged;
        OnCreatureDamaged += HandleCreatureDamaged;
        OnGameOver += HandleGameOver;
    }

    private void InitializeGame() {
        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;
        isGameActive = true;
        NotifyGameStateChanged();
    }

    public void PlayCard(CardData cardData, Player player) {
        if (!isGameActive) return;

        Card card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        NotifyGameStateChanged();
    }

    public void AttackWithCreature(Creature attacker, Player attackingPlayer, Creature target = null) {
        if (!isGameActive) return;

        if (target == null) {
            // Attack player directly
            attackingPlayer.Opponent.TakeDamage(attacker.Attack);
            OnPlayerDamaged?.Invoke(attackingPlayer.Opponent);
        } else {
            // Attack creature
            target.TakeDamage(attacker.Attack, GameContext);
            attacker.TakeDamage(target.Attack, GameContext);
            OnCreatureDamaged?.Invoke(target);
            OnCreatureDamaged?.Invoke(attacker);

            // Clean up dead creatures
            CleanupDeadCreatures(Player1);
            CleanupDeadCreatures(Player2);
        }

        NotifyGameStateChanged();
    }

    private void CleanupDeadCreatures(Player player) {
        player.Battlefield.RemoveAll(creature => creature.Health <= 0);
    }

    public void NotifyGameStateChanged() {
        if (CheckGameEnd()) {
            isGameActive = false;
        }
        OnGameStateChanged?.Invoke();
    }

    private void UpdateGameState() {
        if (gameUI != null) {
            gameUI.UpdateUI();
        }

        if (damageResolutionController != null) {
            damageResolutionController.UpdateResolutionState();
        }
    }

    private void HandlePlayerDamaged(Player player) {
        if (gameUI != null) {
            gameUI.UpdatePlayerHealth(player);
        }
    }

    private void HandleCreatureDamaged(Creature creature) {
        if (gameUI != null) {
            gameUI.UpdateCreatureState(creature);
        }
    }

    private void HandleGameOver(Player winner) {
        if (gameUI != null) {
            gameUI.ShowGameOver(winner);
        }
    }

    public void RestartGame() {
        InitializeGame();
    }

    public bool CheckGameEnd() {
        if (Player1.Health <= 0) {
            OnGameOver?.Invoke(Player2);
            return true;
        }
        if (Player2.Health <= 0) {
            OnGameOver?.Invoke(Player1);
            return true;
        }
        return false;
    }

    private void OnDestroy() {
        // Clean up event listeners
        OnGameStateChanged -= UpdateGameState;
        OnPlayerDamaged -= HandlePlayerDamaged;
        OnCreatureDamaged -= HandleCreatureDamaged;
        OnGameOver -= HandleGameOver;
    }
}