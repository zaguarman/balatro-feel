using UnityEngine;

public class GameUI : Singleton<GameUI> {
    // Public accessors for UI components
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public BattlefieldUI GetBattlefieldUI() => battlefieldUI;

    // Utility method to check initialization status
    public bool IsInitialized() => initialized;

    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private BattlefieldUI battlefieldUI;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BattlefieldUI player1BattlefieldUI;
    [SerializeField] private BattlefieldUI player2BattlefieldUI;

    private GameMediator gameMediator;
    private bool initialized;

    protected override void Awake() {
        base.Awake();
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        gameManager = GameManager.Instance;
        gameMediator = GameMediator.Instance;

        if (gameMediator != null) {
            gameMediator.RegisterUI(this);
        }

        RegisterEvents();
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.UnregisterUI(this);
        }
    }

    public void SetUIReferences(PlayerUI player1UI, PlayerUI player2UI, BattlefieldUI player1BattlefieldUI, BattlefieldUI player2BattlefieldUI) {
        this.player1UI = player1UI;
        this.player2UI = player2UI;
        this.player1BattlefieldUI = battlefieldUI;
        this.player2BattlefieldUI = battlefieldUI;

        if (!initialized) {
            InitializeUI();
        }
    }

    private void InitializeUI() {
        if (!initialized && gameManager != null) {
            SetupPlayerUIs();
            UpdateUI();
            initialized = true;
            Debug.Log("GameUI initialized");
        } else if (gameManager == null) {
            Debug.LogError("GameManager reference is missing");
        }
    }

    private void SetupPlayerUIs() {
        if (player1UI != null && gameManager.Player1 != null) {
            player1UI.Initialize(gameManager.Player1);
            Debug.Log("Player1 UI initialized");
        } else {
            Debug.LogWarning("Player1 UI reference or Player1 is missing");
        }

        if (player2UI != null && gameManager.Player2 != null) {
            player2UI.Initialize(gameManager.Player2);
            Debug.Log("Player2 UI initialized");
        } else {
            Debug.LogWarning("Player2 UI reference or Player2 is missing");
        }
    }

    public void UpdateUI() {
        if (!initialized) {
            Debug.LogWarning("Attempting to update UI before initialization");
            return;
        }

        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
        battlefieldUI?.UpdateUI();
    }

    protected override void OnDestroy() {
        UnregisterEvents();
        if (instance == this) {
            instance = null;
        }
        base.OnDestroy();
    }
}