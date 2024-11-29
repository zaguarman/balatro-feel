using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : Singleton<GameReferences> {
    [Header("Damage Resolution")]
    [SerializeField] private Button resolveActionsButton;
    [Header("Player 1 UI")]
    [SerializeField] private PlayerUI player1UI;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private CardContainer player1Hand;
    [SerializeField] private CardContainer player1Battlefield;
    [SerializeField] private BattlefieldUI player1BattlefieldUI;
    [SerializeField] private HandUI player1HandUI;
    [SerializeField] private HealthUI player1HealthUI;
    [Header("Player 2 UI")]
    [SerializeField] private PlayerUI player2UI;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private CardContainer player2Hand;
    [SerializeField] private CardContainer player2Battlefield;
    [SerializeField] private BattlefieldUI player2BattlefieldUI;
    [SerializeField] private HandUI player2HandUI;
    [SerializeField] private HealthUI player2HealthUI;
    [Header("Card Components")]
    [SerializeField] private Button cardPrefab;
    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    public override void Initialize() {
        if (IsInitialized) return;

        ValidateReferences();
        base.Initialize();
    }

    private void ValidateReferences() {
        if (GetPlayer1UI() == null) Debug.LogError("Player1UI reference missing!");
        if (GetPlayer2UI() == null) Debug.LogError("Player2UI reference missing!");
        if (GetCardPrefab() == null) Debug.LogError("CardPrefab reference missing!");
    }

    public PlayerUI GetPlayer1UI() => player1UI;
    public PlayerUI GetPlayer2UI() => player2UI;
    public CardContainer GetPlayer1Battlefield() => player1Battlefield;
    public CardContainer GetPlayer2Battlefield() => player2Battlefield;
    public CardContainer GetPlayer1Hand() => player1Hand;
    public CardContainer GetPlayer2Hand() => player2Hand;
    public Button GetCardPrefab() => cardPrefab;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public TextMeshProUGUI GetPlayer1HealthText() => player1HealthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2HealthText;
    public BattlefieldUI GetPlayer1BattlefieldUI() => player1BattlefieldUI;
    public BattlefieldUI GetPlayer2BattlefieldUI() => player2BattlefieldUI;
    public HandUI GetPlayer1HandUI() => player1HandUI;
    public HandUI GetPlayer2HandUI() => player2HandUI;
    public HealthUI GetPlayer1HealthUI() => player1HealthUI;
    public HealthUI GetPlayer2HealthUI() => player2HealthUI;
}