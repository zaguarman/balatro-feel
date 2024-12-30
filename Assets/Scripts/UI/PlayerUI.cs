using UnityEngine;

public class PlayerUI : UIComponent {
    private HandUI handUI;

    public override void Initialize(IPlayer player) {
        base.Initialize(player);

        if (gameReferences == null) {
            Debug.LogError("GameReferences not found during PlayerUI initialization");
            return;
        }

        InitializeHandUI(Player);

        IsInitialized = true;
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            //gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            //gameMediator.RemoveGameStateChangedListener(UpdateUI);
        }
    }

    public override void UpdateUI(IPlayer player) {
        if (!IsInitialized || player != Player) return;
        handUI?.UpdateUI(player);
    }


    private void InitializeHandUI(IPlayer player) {
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

    protected override void OnDestroy() {
        base.OnDestroy();
    }
}