using TMPro;
using UnityEngine;

public class GameUI : Singleton<GameUI> {
    private PlayerUI player1UI;
    private PlayerUI player2UI;
    private DamageResolutionUI damageResolutionUI;
    private GameReferences references;
    private IGameMediator gameMediator;

    protected override void Awake() {
        base.Awake(); // This handles the singleton pattern and DontDestroyOnLoad
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        references = GameReferences.Instance;
        gameMediator = GameMediator.Instance;
        InitializeUIComponents();
    }

    private void InitializeUIComponents() {
        // Create Canvas GameObject if it doesn't exist
        Canvas mainCanvas = GetComponentInChildren<Canvas>();
        if (mainCanvas == null) {
            GameObject canvasObject = new GameObject("MainCanvas");
            canvasObject.transform.SetParent(transform);
            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create UI hierarchy
        CreateUIHierarchy(mainCanvas.transform);

        // Get references from hierarchy
        player1UI = references.GetPlayer1UI();
        player2UI = references.GetPlayer2UI();
        damageResolutionUI = references.GetDamageResolutionUI();

        ValidateComponents();

        // Register with GameMediator
        gameMediator.RegisterUI(this);
    }

    private void CreateUIHierarchy(Transform canvasTransform) {
        // Player 1 UI
        GameObject player1UIObj = CreateUIElement("Player1UI", canvasTransform);
        PlayerUI p1UI = player1UIObj.AddComponent<PlayerUI>();
        p1UI.SetIsPlayer1(true);

        // Player 2 UI
        GameObject player2UIObj = CreateUIElement("Player2UI", canvasTransform);
        PlayerUI p2UI = player2UIObj.AddComponent<PlayerUI>();
        p2UI.SetIsPlayer1(false);

        // Damage Resolution UI
        GameObject damageResolutionObj = CreateUIElement("DamageResolutionUI", canvasTransform);
        damageResolutionObj.AddComponent<DamageResolutionUI>();

        // Create UI elements for each section
        CreatePlayerUIElements(player1UIObj.transform, "Player1");
        CreatePlayerUIElements(player2UIObj.transform, "Player2");
        CreateDamageResolutionElements(damageResolutionObj.transform);
    }

    private GameObject CreateUIElement(string name, Transform parent) {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        return obj;
    }

    private void CreatePlayerUIElements(Transform parent, string playerPrefix) {
        // Health Text
        GameObject healthText = CreateUIElement($"{playerPrefix}HealthText", parent);
        TextMeshProUGUI tmpHealth = healthText.AddComponent<TextMeshProUGUI>();
        tmpHealth.text = "Health: 20";
        tmpHealth.fontSize = 24;
        RectTransform healthRect = healthText.GetComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0, 1);
        healthRect.anchorMax = new Vector2(0, 1);
        healthRect.pivot = new Vector2(0, 1);
        healthRect.sizeDelta = new Vector2(200, 50);

        // Hand Container
        GameObject handContainer = CreateUIElement($"{playerPrefix}Hand", parent);
        RectTransform handRect = handContainer.GetComponent<RectTransform>();
        handRect.anchorMin = new Vector2(0.5f, playerPrefix == "Player1" ? 0 : 0.6f);
        handRect.anchorMax = new Vector2(0.5f, playerPrefix == "Player1" ? 0.4f : 1);

        // Battlefield Container
        GameObject battlefieldContainer = CreateUIElement($"{playerPrefix}Battlefield", parent);
        RectTransform battleRect = battlefieldContainer.GetComponent<RectTransform>();
        battleRect.anchorMin = new Vector2(0, playerPrefix == "Player1" ? 0.4f : 0.2f);
        battleRect.anchorMax = new Vector2(1, playerPrefix == "Player1" ? 0.6f : 0.4f);
    }

    private void CreateDamageResolutionElements(Transform parent) {
        // Resolve Button
        GameObject buttonObj = CreateUIElement("ResolveButton", parent);
        UnityEngine.UI.Button resolveButton = buttonObj.AddComponent<UnityEngine.UI.Button>();

        // Button Text
        GameObject buttonTextObj = CreateUIElement("ButtonText", buttonObj.transform);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Resolve Actions";
        buttonText.alignment = TextAlignmentOptions.Center;

        // Pending Actions Text
        GameObject pendingTextObj = CreateUIElement("PendingActionsText", parent);
        TextMeshProUGUI pendingText = pendingTextObj.AddComponent<TextMeshProUGUI>();
        pendingText.text = "Pending Actions: 0";
        pendingText.fontSize = 20;
    }

    private void ValidateComponents() {
        if (player1UI == null) Debug.LogError("Player 1 UI component missing");
        if (player2UI == null) Debug.LogError("Player 2 UI component missing");
        if (damageResolutionUI == null) Debug.LogError("Damage Resolution UI component missing");
    }

    protected override void OnDestroy() {
        if (instance == this) {
            if (gameMediator != null) {
                gameMediator.UnregisterUI(this);
            }
            instance = null;
        }
    }

    // Public method to handle game state updates if needed
    public void UpdateUI() {
        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
        damageResolutionUI?.UpdateUI();
    }
}