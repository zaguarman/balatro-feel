using UnityEngine;
using UnityEngine.Events;
using System;

public class GameManager : Singleton<GameManager> {
    // Game state properties
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    // Core system references
    private IGameMediator gameMediator;
    private GameEvents gameEvents;
    private bool isInitialized;

    // Component references
    public GameUI GameUI => GameUI.Instance;
    public DamageResolver DamageResolver => DamageResolver.Instance;

    // Event accessors that delegate to GameEvents
    public UnityEvent OnGameStateChanged => gameEvents.OnGameStateChanged;
    public GameEvents.PlayerDamagedEvent OnPlayerDamaged => gameEvents.OnPlayerDamaged;
    public GameEvents.CreatureDamagedEvent OnCreatureDamaged => gameEvents.OnCreatureDamaged;
    public GameEvents.CreatureDiedEvent OnCreatureDied => gameEvents.OnCreatureDied;
    public GameEvents.GameOverEvent OnGameOver => gameEvents.OnGameOver;

    protected override void Awake() {
        base.Awake();
        InitializeGameSystems();
    }

    private void InitializeGameSystems() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;
    }

    public void Start() {
        if (!isInitialized) {
            InitializeGame();
            isInitialized = true;
        }
    }

    private void InitializeGame() {
        try {
            GameContext = new GameContext();
            Player1 = new Player();
            Player2 = new Player();
            Player1.Opponent = Player2;
            Player2.Opponent = Player1;

            gameMediator.RegisterPlayer(Player1);
            gameMediator.RegisterPlayer(Player2);

            GameUI gameUI = GameUI.Instance;
            if (gameUI != null) {
                gameMediator.RegisterUI(gameUI);
            }

            if (DamageResolver != null) {
                gameMediator.RegisterDamageResolver(DamageResolver);
            }

            NotifyGameStateChanged();

            Debug.Log("GameManager initialized successfully");
        } catch (Exception e) {
            Debug.LogError($"Error initializing GameManager: {e.Message}");
            isInitialized = false;
        }
    }

    // Card playing functionality
    public void PlayCard(CardData cardData, IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        Debug.Log($"Playing card: {cardData.cardName} for player {(player == Player1 ? "1" : "2")}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    // Event notification methods
    public void NotifyGameStateChanged() => gameEvents.NotifyGameStateChanged();
    public void NotifyPlayerDamaged(IPlayer player, int damage) => gameEvents.NotifyPlayerDamaged(player, damage);
    public void NotifyCreatureDamaged(ICreature creature, int damage) => gameEvents.NotifyCreatureDamaged(creature, damage);
    public void NotifyCreatureDied(ICreature creature) => gameEvents.NotifyCreatureDied(creature);
    public void NotifyGameOver(IPlayer winner) => gameEvents.NotifyGameOver(winner);

    // Game state management
    public void ResetGame() {
        UnregisterCurrentGame();
        isInitialized = false;
        InitializeGame();
    }

    private void UnregisterCurrentGame() {
        if (Player1 != null) gameMediator.UnregisterPlayer(Player1);
        if (Player2 != null) gameMediator.UnregisterPlayer(Player2);
        if (DamageResolver != null) gameMediator.UnregisterDamageResolver(DamageResolver);
        if (GameUI != null) gameMediator.UnregisterUI(GameUI);

        GameContext = null;
        Player1 = null;
        Player2 = null;
    }

    // Scene management
    public void OnSceneLoaded() {
        if (!isInitialized) {
            InitializeGame();
        } else {
            // Refresh UI references if needed
            GameUI.Instance.UpdateUI();
        }
    }

    // Cleanup
    public void OnApplicationQuit() {
        isInitialized = false;
        UnregisterCurrentGame();
        if (gameMediator != null) {
            gameMediator.Cleanup();
        }
    }

    protected override void OnDestroy() {
        if (instance == this) {
            UnregisterCurrentGame();
            if (gameMediator != null) {
                gameMediator.Cleanup();
            }
            instance = null;
        }
        base.OnDestroy();
    }

    // Helper methods for component management
    public T EnsureComponent<T>() where T : Component {
        var component = gameObject.GetComponent<T>();
        if (component == null) {
            component = gameObject.AddComponent<T>();
            Debug.Log($"Added {typeof(T).Name} component");
        }
        return component;
    }

    // Debug methods
    public void LogGameState() {
        Debug.Log($"Game State:");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Player 1 Health: {Player1?.Health}");
        Debug.Log($"Player 2 Health: {Player2?.Health}");
        Debug.Log($"Pending Actions: {GameContext?.GetPendingActionsCount()}");
    }

    // Test setup methods
    public void SetupTestGame() {
        if (!isInitialized) {
            InitializeGame();
        }

        var testSetup = gameObject.GetComponent<TestSetup>() ?? gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();

        // Add test cards to players' hands
        foreach (var card in testCards) {
            var cardInstance = CardFactory.CreateCard(card);
            if (UnityEngine.Random.value > 0.5f) {
                Player1.AddToHand(cardInstance);
            } else {
                Player2.AddToHand(cardInstance);
            }
        }

        NotifyGameStateChanged();
    }

#if UNITY_EDITOR
    // Editor-only validation
    protected void OnValidate() {
        if (Application.isPlaying) return;

        var components = GetComponents<Component>();
        foreach (var component in components) {
            if (component != this && component.GetType().IsSubclassOf(typeof(MonoBehaviour))) {
                Debug.LogWarning($"GameManager should be the only MonoBehaviour on this GameObject. Found: {component.GetType().Name}");
            }
        }
    }
#endif
}