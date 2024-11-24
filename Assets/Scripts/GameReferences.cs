using UnityEngine;
using TMPro;
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

    private Canvas mainCanvas;

    protected override void Awake() {
        base.Awake();
        EnsureCanvasExists();
        ValidateAndCreateReferences();

        InitializeUI();
    }

    private void EnsureCanvasExists() {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null) {
            var canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Created new canvas");
        }
    }

    public void ValidateAndCreateReferences() {
        CreateGameUIIfNeeded();
        CreatePlayerUIComponentsIfNeeded();
        CreateDamageResolutionIfNeeded();
        CreateCardPrefabIfNeeded();
    }

    private void CreateGameUIIfNeeded() {
        if (gameUI == null) {
            var gameUIObj = new GameObject("GameUI");
            gameUIObj.transform.SetParent(mainCanvas.transform, false);
            gameUI = gameUIObj.AddComponent<GameUI>();
            Debug.Log("Created GameUI");
        }
    }

    private void CreatePlayerUIComponentsIfNeeded() {
        // Player 1 Components
        if (player1UI == null) {
            var player1Container = CreatePlayerContainer("Player1Container");
            player1UI = player1Container.AddComponent<PlayerUI>();
        }

        if (player1HealthText == null) {
            player1HealthText = CreateTextComponent("Player1Health", player1UI.transform);
        }

        if (player1Hand == null) {
            player1Hand = CreateContainer("Player1Hand", player1UI.transform);
        }

        if (player1Battlefield == null) {
            player1Battlefield = CreateContainer("Player1Battlefield", player1UI.transform);
        }

        // Player 2 Components
        if (player2UI == null) {
            var player2Container = CreatePlayerContainer("Player2Container");
            player2UI = player2Container.AddComponent<PlayerUI>();
        }

        if (player2HealthText == null) {
            player2HealthText = CreateTextComponent("Player2Health", player2UI.transform);
        }

        if (player2Hand == null) {
            player2Hand = CreateContainer("Player2Hand", player2UI.transform);
        }

        if (player2Battlefield == null) {
            player2Battlefield = CreateContainer("Player2Battlefield", player2UI.transform);
        }
    }

    private void CreateDamageResolutionIfNeeded() {
        if (resolveActionsButton == null) {
            var buttonObj = new GameObject("ResolveActionsButton");
            buttonObj.transform.SetParent(mainCanvas.transform, false);
            resolveActionsButton = buttonObj.AddComponent<Button>();
            var buttonText = CreateTextComponent("ButtonText", buttonObj.transform);
            buttonText.text = "Resolve Actions";
        }

        if (pendingActionsText == null) {
            pendingActionsText = CreateTextComponent("PendingActionsText", mainCanvas.transform);
        }
    }

    private void CreateCardPrefabIfNeeded() {
        if (cardButtonPrefab == null) {
            var cardObj = new GameObject("CardButtonPrefab");
            cardButtonPrefab = cardObj.AddComponent<Button>();
            var cardController = cardObj.AddComponent<CardButtonController>();
            cardObj.SetActive(false); // Prefab should be inactive
            Debug.Log("Created card button prefab");
        }
    }

    private GameObject CreatePlayerContainer(string name) {
        var container = new GameObject(name);
        container.transform.SetParent(mainCanvas.transform, false);
        var rect = container.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return container;
    }

    private TextMeshProUGUI CreateTextComponent(string name, Transform parent) {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        var rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        return text;
    }

    private RectTransform CreateContainer(string name, Transform parent) {
        var container = new GameObject(name);
        container.transform.SetParent(parent, false);
        var rect = container.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    public void InitializeUI() {
        // Add debug logging
        Debug.Log("Initializing UI references");

        if (player1UI != null) {
            var player1Refs = new PlayerUIReferences {
                healthText = player1HealthText,
                handContainer = player1Hand,
                battlefieldContainer = player1Battlefield
            };
            player1UI.SetReferences(player1Refs);
            player1UI.SetIsPlayer1(true);
            Debug.Log("Player1 UI references initialized");
        } else {
            Debug.LogError("Player1 UI component is null");
        }

        if (player2UI != null) {
            var player2Refs = new PlayerUIReferences {
                healthText = player2HealthText,
                handContainer = player2Hand,
                battlefieldContainer = player2Battlefield
            };
            player2UI.SetReferences(player2Refs);
            player2UI.SetIsPlayer1(false);
            Debug.Log("Player2 UI references initialized");
        } else {
            Debug.LogError("Player2 UI component is null");
        }
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
}