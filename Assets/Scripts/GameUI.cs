using UnityEngine;

public class GameUI : Singleton<GameUI> {
    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private BattlefieldUI battlefieldUI;

    private GameMediator gameMediator;
    private bool initialized;

    protected override void Awake() {
        base.Awake();
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;

        if (gameMediator != null) {
            gameMediator.RegisterUI(this);
        }

        RegisterEvents();
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.UnregisterUI(this);
        }
    }

    public void SetUIReferences(PlayerUI player1UI, PlayerUI player2UI, BattlefieldUI battlefieldUI) {
        this.player1UI = player1UI;
        this.player2UI = player2UI;
        this.battlefieldUI = battlefieldUI;

        if (!initialized) {
            InitializeUI();
        }
    }

    private void InitializeUI() {
        if (!initialized) {
            SetupPlayerUIs();
            UpdateUI();
            initialized = true;
            Debug.Log("GameUI initialized");
        }
    }

    private void SetupPlayerUIs() {
        if (player1UI != null) {
            player1UI.SetIsPlayer1(true);
            Debug.Log("Player1 UI initialized");
        } else {
            Debug.LogWarning("Player1 UI reference missing");
        }

        if (player2UI != null) {
            player2UI.SetIsPlayer1(false);
            Debug.Log("Player2 UI initialized");
        } else {
            Debug.LogWarning("Player2 UI reference missing");
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

    // Public accessors for UI components
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public BattlefieldUI GetBattlefieldUI() => battlefieldUI;

    // Utility method to check initialization status
    public bool IsInitialized() => initialized;
}