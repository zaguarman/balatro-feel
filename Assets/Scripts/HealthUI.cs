using TMPro;

public class HealthUI : UIComponent {
    private HealthHandler healthHandler;

    public void Initialize(TextMeshProUGUI healthText, IPlayer player) {
        healthHandler = new HealthHandler(healthText, player, gameMediator);
        UpdateUI();
    }

    protected override void RegisterEvents() {
        // Events are handled by HealthHandler
    }

    protected override void UnregisterEvents() {
        // Events are handled by HealthHandler
    }

    public override void UpdateUI() {
        healthHandler?.UpdateUI();
    }

    protected override void OnDestroy() {
        healthHandler?.Cleanup();
        healthHandler = null;
        base.OnDestroy();
    }
}