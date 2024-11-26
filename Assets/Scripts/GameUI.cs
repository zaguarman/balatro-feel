using Unity.VisualScripting;
using UnityEngine;

public class GameUI : Singleton<GameUI>, IInitializable {
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public bool IsInitialized { get; private set; }

    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private BattlefieldUI player1BattlefieldUI;
    [SerializeField] private BattlefieldUI player2BattlefieldUI;

    private GameManager gameManager;
    private GameMediator gameMediator;
    private InitializationManager initManager;

    protected override void Awake() {
        base.Awake();
        initManager = InitializationManager.Instance;
        gameManager = GameManager.Instance;
        gameMediator = GameMediator.Instance;

        // Register this component with the initialization manager
        if (initManager != null) {
            initManager.RegisterComponent(this);
        }

        // Try to initialize immediately if dependencies are ready
        Initialize();
    }

    private void Start() {
        // Attempt initialization again in Start if not already initialized
        if (!IsInitialized) {
            Initialize();
        }
    }

    public void Initialize() {
        if (IsInitialized) return;

        // Ensure all required dependencies are available and initialized
        if (gameManager == null || !gameManager.IsInitialized ||
            gameMediator == null || !gameMediator.IsInitialized ||
            initManager == null) {
            Debug.LogWarning("GameUI initialization delayed - waiting for dependencies");
            return;
        }

        // Register with GameMediator
        gameMediator.RegisterUI(this);

        // Setup UI elements
        SetupPlayerUIs();

        // Register event listeners
        RegisterEvents();

        IsInitialized = true;
        initManager.MarkComponentInitialized(this);
        Debug.Log("GameUI successfully initialized");
    }

    private void SetupPlayerUIs() {
        if (gameManager == null) return;

        if (player1UI != null && gameManager.Player1 != null) {
            player1UI.Initialize(gameManager.Player1);
        }

        if (player2UI != null && gameManager.Player2 != null) {
            player2UI.Initialize(gameManager.Player2);
        }

        if (player1BattlefieldUI != null && gameManager.Player1 != null) {
            player1BattlefieldUI.Initialize(GameReferences.Instance.GetPlayer1Battlefield(), gameManager.Player1);
        }

        if (player2BattlefieldUI != null && gameManager.Player2 != null) {
            player2BattlefieldUI.Initialize(GameReferences.Instance.GetPlayer2Battlefield(), gameManager.Player2);
        }
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddGameInitializedListener(OnGameInitialized);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveGameInitializedListener(OnGameInitialized);
            gameMediator.UnregisterUI(this);
        }
    }

    private void OnGameInitialized() {
        if (!IsInitialized) {
            Initialize();
        }
        UpdateUI();
    }

    public void UpdateUI() {
        if (!IsInitialized) {
            Initialize();
            if (!IsInitialized) return;
        }

        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
        player1BattlefieldUI?.UpdateUI();
        player2BattlefieldUI?.UpdateUI();
    }

    protected override void OnDestroy() {
        UnregisterEvents();
        if (instance == this) {
            instance = null;
        }
        base.OnDestroy();
    }
}