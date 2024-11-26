using TMPro;
using UnityEngine;

public class HealthUI : UIComponent {
    [SerializeField] private TextMeshProUGUI healthText;
    private bool isPlayer1;
    private IPlayer player;
    private GameMediator gameMediator;

    public void Initialize(TextMeshProUGUI healthText, bool isPlayer1) {
        this.healthText = healthText;
        this.isPlayer1 = isPlayer1;
        this.gameMediator = GameMediator.Instance;
        InitializePlayer();
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        InitializePlayer();
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddPlayerDamagedListener(HandlePlayerDamaged);
            gameMediator.AddGameInitializedListener(InitializePlayer);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemovePlayerDamagedListener(HandlePlayerDamaged);
            gameMediator.RemoveGameInitializedListener(InitializePlayer);
        }
    }

    private void InitializePlayer() {
        var gameManager = GameManager.Instance;
        if (gameManager != null) {
            player = isPlayer1 ? gameManager.Player1 : gameManager.Player2;
            UpdateUI();
        }
    }

    public override void UpdateUI() {
        if (healthText != null && player != null) {
            healthText.text = $"Health: {player.Health}";
        }
    }

    private void HandlePlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            UpdateUI();
        }
    }
}