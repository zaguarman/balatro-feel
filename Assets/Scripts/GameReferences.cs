using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameReferences : Singleton<GameReferences> {
    [Header("Main UI Components")]
    [SerializeField] private GameUI gameUI;

    [Header("Damage Resolution")]
    [SerializeField] private Button resolveActionsButton;

    [Header("Player 1 UI")]
    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private CardContainer player1Hand;
    [SerializeField] private CardContainer player1Battlefield;

    [Header("Player 2 UI")]
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private CardContainer player2Hand;
    [SerializeField] private CardContainer player2Battlefield;

    [Header("Card Components")]
    [SerializeField] private Button cardButtonPrefab;

    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    // Getters
    public GameUI GetGameUI() => gameUI;
    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public CardContainer GetPlayer1Battlefield() => player1Battlefield;
    public CardContainer GetPlayer2Battlefield() => player2Battlefield;
    public CardContainer GetPlayer1Hand() => player1Hand;
    public CardContainer GetPlayer2Hand() => player2Hand;
    public Button GetCardButtonPrefab() => cardButtonPrefab;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public TextMeshProUGUI GetPlayer1HealthText() => player1HealthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2HealthText;
}