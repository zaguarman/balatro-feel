using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : Singleton<GameReferences> {
    [Header("Main UI Components")]
    [SerializeField] private GameUI gameUI;

    [Header("Damage Resolution")]
    [SerializeField] private Button resolveActionsButton;
    [SerializeField] private TextMeshProUGUI pendingActionsText;

    [Header("Player 1 UI")]
    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private RectTransform player1Hand;
    [SerializeField] private RectTransform player1Battlefield;

    [Header("Player 2 UI")]
    [SerializeField] private PlayerUI player2UI;
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
        InitializePlayerUIs();
        ValidateReferences();
    }

    private void InitializePlayerUIs() {
        if (player1UI != null) {
            player1UI.SetIsPlayer1(true);
            SetupPlayerUIReferences(player1UI, player1HealthText, player1Hand, player1Battlefield);
        }

        if (player2UI != null) {
            player2UI.SetIsPlayer1(false);
            SetupPlayerUIReferences(player2UI, player2HealthText, player2Hand, player2Battlefield);
        }

        gameUI?.SetPlayerUIs(player1UI, player2UI);
    }

    private void SetupPlayerUIReferences(PlayerUI playerUI, TextMeshProUGUI healthText,
        RectTransform hand, RectTransform battlefield) {
        var references = new PlayerUIReferences {
            healthText = healthText,
            handContainer = hand,
            battlefieldContainer = battlefield
        };
        playerUI.SetReferences(references);
    }

    // Getters
    public GameUI GetGameUI() => gameUI;
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public RectTransform GetPlayer1Battlefield() => player1Battlefield;
    public RectTransform GetPlayer2Battlefield() => player2Battlefield;
    public Button GetCardButtonPrefab() => cardButtonPrefab;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public TextMeshProUGUI GetPendingActionsText() => pendingActionsText;

    private void ValidateReferences() {
        ValidateMainComponents();
        ValidateDamageResolutionComponents();
        ValidatePlayer1Components();
        ValidatePlayer2Components();
        ValidateCardComponents();
    }

    private void ValidateMainComponents() {
        if (gameUI == null) Debug.LogError("GameUI reference is missing");
        if (player1UI == null) Debug.LogError("Player1 UI reference is missing");
        if (player2UI == null) Debug.LogError("Player2 UI reference is missing");
    }

    private void ValidateDamageResolutionComponents() {
        if (resolveActionsButton == null) Debug.LogError("Resolve Actions Button is missing");
        if (pendingActionsText == null) Debug.LogError("Pending Damage Text is missing");
    }

    private void ValidatePlayer1Components() {
        if (player1HealthText == null) Debug.LogError("Player1 Health Text is missing");
        if (player1Hand == null) Debug.LogError("Player1 Hand is missing");
        if (player1Battlefield == null) Debug.LogError("Player1 Battlefield is missing");
    }

    private void ValidatePlayer2Components() {
        if (player2HealthText == null) Debug.LogError("Player2 Health Text is missing");
        if (player2Hand == null) Debug.LogError("Player2 Hand is missing");
        if (player2Battlefield == null) Debug.LogError("Player2 Battlefield is missing");
    }

    private void ValidateCardComponents() {
        if (cardButtonPrefab == null) Debug.LogError("Card Button Prefab is missing");
    }
}