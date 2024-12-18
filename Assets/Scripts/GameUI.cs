using UnityEngine;

public class GameUI : UIComponent {
    private static GameUI instance;
    public static GameUI Instance => instance;

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

        gameManager = GameManager.Instance;
        GetReferences();
        InitializeUI();
        base.Initialize();
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
        if (player1BattlefieldUI != null) player1BattlefieldUI.Initialize(gameManager.Player1);
        if (player2BattlefieldUI != null) player2BattlefieldUI.Initialize(gameManager.Player2);
    }

    public override void OnGameStateChanged() {
        UpdateUI();
    }

    public override void OnGameInitialized() {
        InitializeUI();
        UpdateUI();
    }

    public override void UpdateUI() {
        if (!IsInitialized) return;
        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
        player1BattlefieldUI?.UpdateUI();
        player2BattlefieldUI?.UpdateUI();
    }

    protected override void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
        base.OnDestroy();
    }
}