using TMPro;

public class HealthHandler {
    private readonly TextMeshProUGUI healthText;
    private readonly IPlayer player;

    public HealthHandler(TextMeshProUGUI healthText, IPlayer player) {
        this.healthText = healthText;
        this.player = player;
    }

    public void UpdateUI() {
        if (healthText != null && player != null) {
            healthText.text = $"Health: {player.Health}";
        }
    }

    public void Cleanup() {
        // No need for cleanup since we're not using observer pattern anymore
    }
}
