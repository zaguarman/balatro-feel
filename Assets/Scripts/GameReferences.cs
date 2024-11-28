using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : InitializableComponent {
    private static GameReferences instance;
    public static GameReferences Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameReferences>();
                if (instance == null) {
                    Debug.LogError("GameReferences not found in scene!");
                }
            }
            return instance;
        }
    }

    [Header("Main UI Components")]
    [SerializeField] private GameUI gameUI;

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
    [SerializeField] private Button cardButtonPrefab;

    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    protected override void Awake() {
        base.Awake();
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public override void Initialize() {
        if (IsInitialized) return;

        ValidateReferences();
        base.Initialize();
    }

    private void ValidateReferences() {
        if (gameUI == null) Debug.LogError("GameUI reference missing!");
        if (player1UI == null) Debug.LogError("Player1UI reference missing!");
        if (player2UI == null) Debug.LogError("Player2UI reference missing!");
        if (cardButtonPrefab == null) Debug.LogError("CardButtonPrefab reference missing!");
    }

    // Getters for all components
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
    public BattlefieldUI GetPlayer1BattlefieldUI() => player1BattlefieldUI;
    public BattlefieldUI GetPlayer2BattlefieldUI() => player2BattlefieldUI;
    public HandUI GetPlayer1HandUI() => player1HandUI;
    public HandUI GetPlayer2HandUI() => player2HandUI;
    public HealthUI GetPlayer1HealthUI() => player1HealthUI;
    public HealthUI GetPlayer2HealthUI() => player2HealthUI;

    private void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
}