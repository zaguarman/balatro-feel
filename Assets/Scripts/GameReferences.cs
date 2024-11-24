using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : Singleton<GameReferences> {
    [Header("Main UI Components")]
    [SerializeField] private GameUI gameUI;

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
        ValidateReferences();
    }

    // Getters
    public GameUI GetGameUI() => gameUI;
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public TextMeshProUGUI GetPlayer1HealthText() => player1HealthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2HealthText;
    public RectTransform GetPlayer1Hand() => player1Hand;
    public RectTransform GetPlayer2Hand() => player2Hand;
    public RectTransform GetPlayer1Battlefield() => player1Battlefield;
    public RectTransform GetPlayer2Battlefield() => player2Battlefield;
    public Button GetCardButtonPrefab() => cardButtonPrefab;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;

    private void ValidateReferences() {
        if (gameUI == null) Debug.LogError("GameUI reference is missing");
        if (player1UI == null) Debug.LogError("Player1 UI reference is missing");
        if (player2UI == null) Debug.LogError("Player2 UI reference is missing");
        if (player1HealthText == null) Debug.LogError("Player1 Health Text is missing");
        if (player1Hand == null) Debug.LogError("Player1 Hand is missing");
        if (player1Battlefield == null) Debug.LogError("Player1 Battlefield is missing");
        if (player2HealthText == null) Debug.LogError("Player2 Health Text is missing");
        if (player2Hand == null) Debug.LogError("Player2 Hand is missing");
        if (player2Battlefield == null) Debug.LogError("Player2 Battlefield is missing");
        if (cardButtonPrefab == null) Debug.LogError("Card Button Prefab is missing");
    }
}
