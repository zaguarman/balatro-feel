using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private RectTransform player1CardContainer;
    [SerializeField] private RectTransform player2CardContainer;
    [SerializeField] private Button cardButtonPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;
    [SerializeField] private List<CardData> testCards;

    private IGameMediator gameMediator;
    private DamageResolver damageResolver;
    private GameReferences gameReferences;

    protected void Awake() {
        gameReferences = GameReferences.Instance;
        InitializeReferences();
        gameMediator = GameMediator.Instance;

        var resolveButton = gameReferences.GetResolveActionsButton();
        var pendingDamageText = gameReferences.GetPendingDamageText();
        var gameManager = GameManager.Instance;

        damageResolver = new DamageResolver(
            resolveButton,
            pendingDamageText,
            gameManager
        );
    }

    protected void Start() {
        SetupUI();
    }

    protected void OnEnable() {
        if (gameMediator != null) {
            gameMediator.RegisterUI(this);
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnPlayerDamaged.AddListener(HandlePlayerDamaged);
        }
    }

    protected void OnDisable() {
        if (gameMediator != null) {
            gameMediator.UnregisterUI(this);
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        }
        damageResolver?.Cleanup();
    }

    private void HandlePlayerDamaged(IPlayer player, int damage) {
        UpdatePlayerHealth(player);
    }

    public void UpdateUI() {
        var gameManager = GameManager.Instance;
        UpdatePlayerHealth(gameManager.Player1);
        UpdatePlayerHealth(gameManager.Player2);
        UpdateResolutionUI();
    }

    public void UpdatePlayerHealth(IPlayer player) {
        var gameManager = GameManager.Instance;
        TextMeshProUGUI healthText = player == gameManager.Player1
            ? player1HealthText
            : player2HealthText;

        if (healthText != null) {
            healthText.text = $"Health: {player.Health}";
        }
    }

    private void InitializeReferences() {
        player1HealthText = gameReferences.GetPlayer1HealthText();
        player2HealthText = gameReferences.GetPlayer2HealthText();
        player1CardContainer = gameReferences.GetPlayer1CardContainer();
        player2CardContainer = gameReferences.GetPlayer2CardContainer();
        cardButtonPrefab = gameReferences.GetCardButtonPrefab();
    }

    private void SetupCardContainer(RectTransform container) {
        var existingLayout = container.GetComponent<LayoutGroup>();
        if (existingLayout != null) {
            Destroy(existingLayout);
        }

        float totalWidth = cardOffset + (cardSpacing * testCards.Count);
        container.sizeDelta = new Vector2(totalWidth, 320f);
    }

    public void CreateCardButtons() {
        SetupCardContainer(player1CardContainer);
        SetupCardContainer(player2CardContainer);

        for (int i = 0; i < testCards.Count; i++) {
            CreateCardButton(testCards[i], player1CardContainer, true, i);
            CreateCardButton(testCards[i], player2CardContainer, false, i);
        }
    }

    private void CreateCardButton(CardData cardData, RectTransform parent, bool isPlayer1, int index) {
        Button buttonObj = Instantiate(cardButtonPrefab, parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        float xPos = cardOffset + (cardSpacing * index);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(xPos, 0);

        CardButtonController controller = buttonObj.GetComponent<CardButtonController>();
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(
                cardData,
                isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2
            );
        });
    }

    private void SetupUI() {
        LoadTestCards();
        CreateCardButtons();
        UpdateResolutionUI();
    }

    private void LoadTestCards() {
        if (testCards == null || testCards.Count == 0) {
            var testSetup = GetComponent<TestSetup>();
            if (testSetup == null) {
                testSetup = gameObject.AddComponent<TestSetup>();
            }
            testCards = testSetup.CreateTestCards();
        }
    }

    private void UpdateResolutionUI() {
        damageResolver?.UpdateResolutionState();
    }

    protected void OnDestroy() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        }
        damageResolver?.Cleanup();
        gameReferences = null;
    }
}