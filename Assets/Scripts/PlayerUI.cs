using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour {
    private IPlayer player;
    private bool isPlayer1;
    private GameMediator gameMediator;
    private bool isInitialized;
    private GameReferences gameReferences;
    private HandUI handUI;
    private HealthUI healthUI;

    public void Start() {
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;
        gameReferences = GameReferences.Instance;

        // Initialize child components
        InitializeHealthUI();
        InitializeHandUI();

        RegisterEvents();
    }

    private void InitializeHealthUI() {
        if (healthUI == null) {
            healthUI = gameObject.AddComponent<HealthUI>();
            // Pass down the reference from GameReferences
            var healthText = isPlayer1 ?
                gameReferences.GetPlayer1HealthText() :
                gameReferences.GetPlayer2HealthText();
            healthUI.Initialize(healthText, isPlayer1);
        }
    }

    private void InitializeHandUI() {
        if (handUI == null) {
            handUI = gameObject.AddComponent<HandUI>();
            handUI.SetIsPlayer1(isPlayer1);
        }
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddPlayerDamagedListener(HandlePlayerDamaged);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.RemovePlayerDamagedListener(HandlePlayerDamaged);
        }
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        // Pass down to child components
        if (handUI != null) {
            handUI.SetIsPlayer1(value);
        }
        if (healthUI != null) {
            healthUI.SetIsPlayer1(value);
        }
        InitializePlayer();
    }

    private void InitializePlayer() {
        var gameManager = GameManager.Instance;
        if (gameManager != null) {
            player = isPlayer1 ? gameManager.Player1 : gameManager.Player2;
            if (player != null) {
                isInitialized = true;
                UpdateUI();
            }
        }
    }

    public void UpdateUI() {
        if (!isInitialized || player == null) return;
        healthUI?.UpdateUI();
        handUI?.UpdateUI();
    }

    private void HandlePlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            healthUI?.UpdateUI();
        }
    }

    private void OnDestroy() {
        UnregisterEvents();
    }

    
}