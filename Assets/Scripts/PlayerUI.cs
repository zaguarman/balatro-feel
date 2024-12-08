using UnityEngine;

public class PlayerUI : UIComponent {
    private IPlayer player;
    private HandUI handUI;

    public void Initialize(IPlayer player) {
        this.player = player;

        if (gameReferences == null) {
            Debug.LogError("GameReferences not found during PlayerUI initialization");
            return;
        }

        InitializeHandUI();

        IsInitialized = true;
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;
        handUI?.UpdateUI();
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

    public void SetPlayer(IPlayer player) {
        this.player = player;
        if (handUI != null) {
            handUI.Initialize(player);
        }
        IsInitialized = player != null;
        UpdateUI();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }
}