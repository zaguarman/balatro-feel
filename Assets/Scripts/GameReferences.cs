using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReferences : MonoBehaviour {
    private static GameReferences instance;
    public static GameReferences Instance {
        get {
            if (instance == null) {
                var go = new GameObject("GameReferences");
                instance = go.AddComponent<GameReferences>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("UI")]
    [SerializeField] private Button resolveActionsButton;
    [SerializeField] private TextMeshProUGUI pendingDamageText;

    [Header("Player 1 UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private RectTransform player1CardContainer;
    [SerializeField] private RectTransform player1Battlefield;

    [Header("Player 2 UI")]
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private RectTransform player2CardContainer;
    [SerializeField] private RectTransform player2Battlefield;

    [Header("Card Components")]
    [SerializeField] private Button cardButtonPrefab;

    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    public void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        ValidateReferences();
        InitializeGameMediator();
        ReferenceManager.Initialize(this);
    }

    private void InitializeGameMediator() {
        GameMediator.Instance.Initialize();
    }

    // Original getters
    public TextMeshProUGUI GetPlayer1HealthText() => player1HealthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2HealthText;
    public RectTransform GetPlayer1CardContainer() => player1CardContainer;
    public RectTransform GetPlayer2CardContainer() => player2CardContainer;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public TextMeshProUGUI GetPendingDamageText() => pendingDamageText;
    public RectTransform GetPlayer1Battlefield() => player1Battlefield;
    public RectTransform GetPlayer2Battlefield() => player2Battlefield;
    public Button GetCardButtonPrefab() => cardButtonPrefab;
    public GameMediator GetGameMediator() => GameMediator.Instance;

    // New getters for card components
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;

    public bool ValidateReferences() {
        bool isValid = true;

        // Original validations
        if (resolveActionsButton == null) { Debug.LogError("Resolve Actions Button is missing"); isValid = false; }
        if (pendingDamageText == null) { Debug.LogError("Pending Damage Text is missing"); isValid = false; }
        if (player1HealthText == null) { Debug.LogError("Player1 Health Text is missing"); isValid = false; }
        if (player1CardContainer == null) { Debug.LogError("Player1 Card Container is missing"); isValid = false; }
        if (player1Battlefield == null) { Debug.LogError("Player1 Battlefield is missing"); isValid = false; }
        if (player2HealthText == null) { Debug.LogError("Player2 Health Text is missing"); isValid = false; }
        if (player2CardContainer == null) { Debug.LogError("Player2 Card Container is missing"); isValid = false; }
        if (player2Battlefield == null) { Debug.LogError("Player2 Battlefield is missing"); isValid = false; }
        if (cardButtonPrefab == null) { Debug.LogError("Card Button Prefab is missing"); isValid = false; }

        return isValid;
    }

    public void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
}