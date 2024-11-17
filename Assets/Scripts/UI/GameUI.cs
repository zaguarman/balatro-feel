using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour {
    [Header("Player 1 UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player1BattlefieldText;
    [SerializeField] private RectTransform player1CardContainer;

    [Header("Player 2 UI")]
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private TextMeshProUGUI player2BattlefieldText;
    [SerializeField] private RectTransform player2CardContainer;

    [Header("Card Layout")]
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private float cardSpacing = 220f; // Width + 20 padding
    [SerializeField] private float cardOffset = 50f;   // Initial offset from left
    [SerializeField] private List<CardData> testCards;

    private List<Button> player1Buttons = new List<Button>();
    private List<Button> player2Buttons = new List<Button>();

    private void Awake() {
        // Load test cards if none are assigned
        if (testCards == null || testCards.Count == 0) {
            var testSetup = GetComponent<TestSetup>();
            if (testSetup == null) {
                testSetup = gameObject.AddComponent<TestSetup>();
            }
            testCards = testSetup.CreateTestCards();
        }
    }

    private void Start() {
        GameManager.Instance.OnGameStateChanged += UpdateUI;
        CreateCardButtons();
        UpdateUI();
    }

    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateUI;
        }
    }

    private void SetupCardContainer(RectTransform container) {
        // Remove any existing layout group
        var existingLayout = container.GetComponent<LayoutGroup>();
        if (existingLayout != null) {
            Destroy(existingLayout);
        }

        // Set container size based on cards
        float totalWidth = cardOffset + (cardSpacing * testCards.Count);
        container.sizeDelta = new Vector2(totalWidth, 320f); // Height + padding
    }

    private void CreateCardButton(CardData cardData, RectTransform parent, bool isPlayer1, int index) {
        Button buttonObj = Instantiate(cardButtonPrefab, parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        // Position the card
        float xPos = cardOffset + (cardSpacing * index);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(xPos, 0);

        // Setup the card
        CardButtonController controller = buttonObj.GetComponent<CardButtonController>();
        controller.Setup(cardData, isPlayer1);

        // Add click listener
        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(
                cardData,
                isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2
            );
        });

        // Store button reference
        if (isPlayer1)
            player1Buttons.Add(buttonObj);
        else
            player2Buttons.Add(buttonObj);
    }

    private void CreateCardButtons() {
        // Clear existing buttons
        ClearButtons();

        // Setup containers
        SetupCardContainer(player1CardContainer);
        SetupCardContainer(player2CardContainer);

        // Create cards for both players
        for (int i = 0; i < testCards.Count; i++) {
            CreateCardButton(testCards[i], player1CardContainer, true, i);
            CreateCardButton(testCards[i], player2CardContainer, false, i);
        }
    }

    private void ClearButtons() {
        foreach (var button in player1Buttons) {
            if (button != null)
                Destroy(button.gameObject);
        }
        foreach (var button in player2Buttons) {
            if (button != null)
                Destroy(button.gameObject);
        }

        player1Buttons.Clear();
        player2Buttons.Clear();
    }

    private void UpdateUI() {
        var p1 = GameManager.Instance.Player1;
        var p2 = GameManager.Instance.Player2;

        // Update health
        player1HealthText.text = $"Player 1 HP: {p1.Health}";
        player2HealthText.text = $"Player 2 HP: {p2.Health}";

        // Update battlefield
        player1BattlefieldText.text = "Battlefield: " + string.Join(", ",
            p1.Battlefield.ConvertAll(c => $"{c.Name}({c.Attack}/{c.Health})"));
        player2BattlefieldText.text = "Battlefield: " + string.Join(", ",
            p2.Battlefield.ConvertAll(c => $"{c.Name}({c.Attack}/{c.Health})"));

        // Check game end
        if (CheckGameEnd()) {
            DisableAllButtons();
        }
    }

    private bool CheckGameEnd() {
        var p1 = GameManager.Instance.Player1;
        var p2 = GameManager.Instance.Player2;

        return p1.Health <= 0 || p2.Health <= 0;
    }

    private void DisableAllButtons() {
        foreach (var button in player1Buttons) {
            if (button != null)
                button.interactable = false;
        }
        foreach (var button in player2Buttons) {
            if (button != null)
                button.interactable = false;
        }
    }

    public void RestartGame() {
        GameManager.Instance.RestartGame();
        CreateCardButtons();
        UpdateUI();
    }
}