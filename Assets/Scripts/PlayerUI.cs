using UnityEngine;

public class PlayerUI : UIComponent {
    private IPlayer player;
    private HandUI handUI;
    private HealthHandler healthHandler;

    public void Initialize(IPlayer player) {
        if (IsInitialized) return;
        
        this.player = player;
        if (gameReferences == null) {
            Debug.LogError("GameReferences not found during PlayerUI initialization");
            return;
        }

        InitializeHandUI();
        InitializeHealthHandler();
        
        IsInitialized = true;
        UpdateUI();
    }

    private void InitializeHandUI() {
        if (player == null) {
            Debug.LogError("Player is null on PlayerUI");
            return;
        }

        handUI = player.IsPlayer1() ?
            gameReferences.GetPlayer1HandUI() :
            gameReferences.GetPlayer2HandUI();

        if (handUI != null) {
            handUI.Initialize(player);
        } else {
            Debug.LogError($"HandUI reference missing for {(player.IsPlayer1() ? "Player 1" : "Player 2")}");
        }
    }

    private void InitializeHealthHandler() {
        var healthText = player.IsPlayer1() ?
            gameReferences.GetPlayer1HealthText() :
            gameReferences.GetPlayer2HealthText();

        healthHandler = new HealthHandler(healthText, player);
    }

    public override void OnGameStateChanged() {
        UpdateUI();
    }

    public override void OnPlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            UpdateUI();
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;
        handUI?.UpdateUI();
        healthHandler?.UpdateUI();
    }

    protected override void CleanupComponent() {
        healthHandler?.Cleanup();
    }

    public void SetPlayer(IPlayer player) {
        this.player = player;
        if (handUI != null) {
            handUI.Initialize(player);
        }
        IsInitialized = player != null;
        UpdateUI();
    }
}