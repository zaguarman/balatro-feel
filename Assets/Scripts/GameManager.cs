using UnityEngine;
using System;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameManager>();
                if (instance == null) {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    InitializeRequiredComponents(go);
                }
            }
            return instance;
        }
    }

    // Game state properties
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    // Component references
    public GameUI GameUI { get; private set; }
    public DamageResolver DamageResolver => DamageResolver.Instance;
    public TestSetup TestSetup { get; private set; }
    public BattlefieldUI BattlefieldUI { get; private set; }

    // GameMediator reference
    private IGameMediator gameMediator;

    // Event accessors that delegate to GameMediator
    public UnityEvent OnGameStateChanged => gameMediator.OnGameStateChanged;
    public PlayerDamagedEvent OnPlayerDamaged => gameMediator.OnPlayerDamaged;
    public CreatureDamagedEvent OnCreatureDamaged => gameMediator.OnCreatureDamaged;
    public CreatureDiedEvent OnCreatureDied => gameMediator.OnCreatureDied;
    public GameOverEvent OnGameOver => gameMediator.OnGameOver;

    private bool isInitialized = false;

    private static void InitializeRequiredComponents(GameObject gameObject) {
        var gameManager = gameObject.GetComponent<GameManager>();
        if (gameManager == null) return;

        gameManager.GameUI = gameManager.EnsureComponent<GameUI>();
        gameManager.TestSetup = gameManager.EnsureComponent<TestSetup>();
        gameManager.BattlefieldUI = gameManager.EnsureComponent<BattlefieldUI>();

        Debug.Log("All required components initialized successfully");
    }

    protected void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize GameMediator
        gameMediator = GameMediator.Instance;
        gameMediator.Initialize();
    }

    public void Start() {
        InitializeRequiredComponents(gameObject);

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

            if (GameUI != null) {
                gameMediator.RegisterUI(GameUI);
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

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        Debug.Log($"Playing card: {cardData.cardName}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    public void NotifyGameStateChanged() {
        gameMediator.NotifyGameStateChanged();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        gameMediator.NotifyPlayerDamaged(player, damage);
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        gameMediator.NotifyCreatureDamaged(creature, damage);
    }

    public void NotifyCreatureDied(ICreature creature) {
        gameMediator.NotifyCreatureDied(creature);
    }

    public void NotifyGameOver(IPlayer winner) {
        gameMediator.NotifyGameOver(winner);
    }

    public void ResetGame() {
        gameMediator.Cleanup();
        gameMediator.Initialize();
        isInitialized = false;
        InitializeGame();
    }

    public void OnSceneLoaded() {
        if (!isInitialized) {
            InitializeGame();
        }
    }

    public void OnApplicationQuit() {
        isInitialized = false;
        gameMediator.Cleanup();
    }

    public void OnDestroy() {
        if (instance == this) {
            if (DamageResolver != null) {
                gameMediator.UnregisterDamageResolver(DamageResolver);
                DamageResolver.Cleanup();
            }
            if (GameUI != null) {
                gameMediator.UnregisterUI(GameUI);
            }
            gameMediator.UnregisterPlayer(Player1);
            gameMediator.UnregisterPlayer(Player2);
            gameMediator.Cleanup();
            instance = null;
        }
    }

    // Helper method to add a new component if it doesn't exist
    public T EnsureComponent<T>() where T : Component {
        var component = gameObject.GetComponent<T>();
        if (component == null) {
            component = gameObject.AddComponent<T>();
            Debug.Log($"Added {typeof(T).Name} component");
        }
        return component;
    }
}