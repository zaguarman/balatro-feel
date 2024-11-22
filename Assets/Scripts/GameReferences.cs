using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : Singleton<GameReferences> {
    [Header("Main UI Components")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private DamageResolutionUI damageResolutionUI;

    [Header("Damage Resolution")]
    [SerializeField] private Button resolveActionsButton;
    [SerializeField] private TextMeshProUGUI pendingDamageText;

    [Header("Player 1 UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private RectTransform player1Hand;
    [SerializeField] private RectTransform player1Battlefield;

    [Header("Player 2 UI")]
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private RectTransform player2Hand;
    [SerializeField] private RectTransform player2Battlefield;

    [Header("Card Components")]
    [SerializeField] private Button cardButtonPrefab;

    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    protected override void Awake() {
        base.Awake();
        ValidateReferences();
        SetupGameUI();
    }

    private void SetupGameUI() {
        if (gameUI != null) {
            // Auto-wire the UI components
            var serializedFields = typeof(GameUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in serializedFields) {
                if (field.Name == "player1UI") field.SetValue(gameUI, player1UI);
                if (field.Name == "player2UI") field.SetValue(gameUI, player2UI);
                if (field.Name == "damageResolutionUI") field.SetValue(gameUI, damageResolutionUI);
            }
        }
    }

    // UI Component Getters
    public GameUI GetGameUI() => gameUI;
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public DamageResolutionUI GetDamageResolutionUI() => damageResolutionUI;

    // Original getters
    public TextMeshProUGUI GetPlayer1HealthText() => player1HealthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2HealthText;
    public RectTransform GetPlayer1Hand() => player1Hand;
    public RectTransform GetPlayer2Hand() => player2Hand;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public TextMeshProUGUI GetPendingDamageText() => pendingDamageText;
    public RectTransform GetPlayer1Battlefield() => player1Battlefield;
    public RectTransform GetPlayer2Battlefield() => player2Battlefield;
    public Button GetCardButtonPrefab() => cardButtonPrefab;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;

    public bool ValidateReferences() {
        bool isValid = true;

        // Main UI Components
        if (gameUI == null) { Debug.LogError("GameUI reference is missing"); isValid = false; }
        if (player1UI == null) { Debug.LogError("Player1UI reference is missing"); isValid = false; }
        if (player2UI == null) { Debug.LogError("Player2UI reference is missing"); isValid = false; }
        if (damageResolutionUI == null) { Debug.LogError("DamageResolutionUI reference is missing"); isValid = false; }

        // Damage Resolution
        if (resolveActionsButton == null) { Debug.LogError("Resolve Actions Button is missing"); isValid = false; }
        if (pendingDamageText == null) { Debug.LogError("Pending Damage Text is missing"); isValid = false; }

        // Player 1
        if (player1HealthText == null) { Debug.LogError("Player1 Health Text is missing"); isValid = false; }
        if (player1Hand == null) { Debug.LogError("Player1 Hand is missing"); isValid = false; }
        if (player1Battlefield == null) { Debug.LogError("Player1 Battlefield is missing"); isValid = false; }

        // Player 2
        if (player2HealthText == null) { Debug.LogError("Player2 Health Text is missing"); isValid = false; }
        if (player2Hand == null) { Debug.LogError("Player2 Hand is missing"); isValid = false; }
        if (player2Battlefield == null) { Debug.LogError("Player2 Battlefield is missing"); isValid = false; }

        // Card Components
        if (cardButtonPrefab == null) { Debug.LogError("Card Button Prefab is missing"); isValid = false; }

        return isValid;
    }
}