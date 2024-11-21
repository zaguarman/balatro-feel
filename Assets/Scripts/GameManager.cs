using UnityEngine;
using System;

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
                    DontDestroyOnLoad(go);
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

    // Events
    public event Action OnGameStateChanged;
    public event Action<IPlayer> OnPlayerDamaged;
    public event Action<ICreature> OnCreatureDamaged;
    public event Action<IPlayer> OnGameOver;

    private bool isInitialized = false;

    private static void InitializeRequiredComponents(GameObject gameObject) {
        var gameManager = gameObject.GetComponent<GameManager>();
        if (gameManager == null) return;

        gameManager.GameUI = gameManager.EnsureComponent<GameUI>();
        gameManager.TestSetup = gameManager.EnsureComponent<TestSetup>();
        gameManager.BattlefieldUI = gameManager.EnsureComponent<BattlefieldUI>();

        Debug.Log("All required components initialized successfully");
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

            GameMediator.Instance.RegisterPlayer(Player1);
            GameMediator.Instance.RegisterPlayer(Player2);

            if (GameUI != null) {
                GameMediator.Instance.RegisterUI(GameUI);
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
        OnGameStateChanged?.Invoke();
        GameMediator.Instance?.NotifyGameStateChanged();
    }

    public void ResetGame() {
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
    }

    public void OnDestroy() {
        if (instance == this) {
            DamageResolver?.Cleanup();
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