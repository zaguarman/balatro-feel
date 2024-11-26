using TMPro;
using UnityEngine;

public class HealthUI : UIComponent {
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private IPlayer player;
    [SerializeField] private GameMediator gameMediator;

    public void Initialize(TextMeshProUGUI healthText, IPlayer player) {
        this.healthText = healthText;
        this.player = player;
        this.gameMediator = GameMediator.Instance;
        UpdateUI();
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddPlayerDamagedListener(HandlePlayerDamaged);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemovePlayerDamagedListener(HandlePlayerDamaged);
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