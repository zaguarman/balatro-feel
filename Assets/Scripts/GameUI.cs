using UnityEngine;

public class GameUI : UIComponent {
    private static GameUI instance;
    public static GameUI Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameUI>();
                if (instance == null) {
                    Debug.LogError("GameUI not found in scene!");
                }
            }
            return instance;
        }
    }

    private PlayerUI player1UI;
    private PlayerUI player2UI;
    private BattlefieldUI player1BattlefieldUI;
    private BattlefieldUI player2BattlefieldUI;

    private GameManager gameManager;

    protected override void Awake() {
        base.Awake();
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public override void Initialize() {
        if (IsInitialized) return;

        var initManager = InitializationManager.Instance;
        if (!initManager.IsComponentInitialized<GameReferences>() ||
            !initManager.IsComponentInitialized<GameMediator>() ||
            !initManager.IsComponentInitialized<GameManager>()) {
            Debug.LogWarning("Required dependencies not initialized yet");
            return;
        }

        gameManager = GameManager.Instance;

        GetReferences();
        InitializeUI();
        base.Initialize();  // This will handle RegisterEvents() and UpdateUI()
    }

    private void GetReferences() {
        player1UI = gameReferences.GetPlayer1UI();
        player2UI = gameReferences.GetPlayer2UI();
        player1BattlefieldUI = gameReferences.GetPlayer1BattlefieldUI();
        player2BattlefieldUI = gameReferences.GetPlayer2BattlefieldUI();
    }

    private void InitializeUI() {
        if (player1UI != null) player1UI.Initialize(gameManager.Player1);
        if (player2UI != null) player2UI.Initialize(gameManager.Player2);
        if (player1BattlefieldUI != null) {
            player1BattlefieldUI.Initialize(gameReferences.GetPlayer1Battlefield(), gameManager.Player1);
        }
        if (player2BattlefieldUI != null) {
            player2BattlefieldUI.Initialize(gameReferences.GetPlayer2Battlefield(), gameManager.Player2);
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddGameInitializedListener(OnGameInitialized);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveGameInitializedListener(OnGameInitialized);
        }
    }

    private void OnGameInitialized() {
        UpdateUI();
    }

    public override void UpdateUI() {
        if (!IsInitialized) return;
        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
        player1BattlefieldUI?.UpdateUI();
        player2BattlefieldUI?.UpdateUI();
    }

    private void OnDestroy() {
        UnregisterEvents();
        if (instance == this) {
            instance = null;
        }
    }
}