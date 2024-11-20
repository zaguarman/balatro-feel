using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameUI : MonoBehaviour {
    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private RectTransform player1CardContainer;
    [SerializeField] private RectTransform player2CardContainer;

    [Header("Game State UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;

    [Header("Resolution UI")]
    [SerializeField] private DamageResolver damageResolver;

    [Header("UI Animation")]
    [SerializeField] private float fadeSpeed = 2f;
    private CanvasGroup gameOverCanvasGroup;

    [Header("Card Layout")]
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private float cardSpacing = 220f; // Width + 20 padding
    [SerializeField] private float cardOffset = 50f;   // Initial offset from left
    [SerializeField] private List<CardData> testCards;

    public void Awake() {
        // Add CanvasGroup component programmatically if it doesn't exist
        gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (gameOverCanvasGroup == null) {
            gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
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
    }

    public void CreateCardButtons() {
        // Setup containers
        SetupCardContainer(player1CardContainer);
        SetupCardContainer(player2CardContainer);

        // Create cards for both players
        for (int i = 0; i < testCards.Count; i++) {
            CreateCardButton(testCards[i], player1CardContainer, true, i);
            CreateCardButton(testCards[i], player2CardContainer, false, i);
        }
    }

    public void Start() {
        SetupUI();
        UpdateUI();
    }

    private void SetupUI() {
        if (restartButton != null) {
            restartButton.onClick.AddListener(HandleRestart);
        }

        if (gameOverPanel != null && gameOverCanvasGroup != null) {
            // Initialize game over panel
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
            gameOverPanel.SetActive(true); // Keep panel active but transparent
        } else {
            Debug.LogError("GameOverPanel not assigned!");
        }

        LoadTestCards();

        CreateCardButtons();

        if (damageResolver != null) {
            damageResolver.gameObject.SetActive(true);
        }
    }

    private void LoadTestCards() {
        // Load test cards if none are assigned
        if (testCards == null || testCards.Count == 0) {
            var testSetup = GetComponent<TestSetup>();
            if (testSetup == null) {
                testSetup = gameObject.AddComponent<TestSetup>();
            }
            testCards = testSetup.CreateTestCards();
        }
    }


    public void UpdateUI() {
        UpdatePlayerHealth(GameManager.Instance.Player1);
        UpdatePlayerHealth(GameManager.Instance.Player2);
        UpdateResolutionUI();
    }

    public void UpdatePlayerHealth(Player player) {
        TextMeshProUGUI healthText = player == GameManager.Instance.Player1 ?
            player1HealthText : player2HealthText;

        if (healthText != null) {
            healthText.text = $"Health: {player.Health}";
        }
    }

    public void UpdateCreatureState(Creature creature) {
        // Update the visual state of the creature card
    }

    public void ShowGameOver(Player winner) {
        if (gameOverPanel != null && gameOverText != null) {
            StopAllCoroutines();
            StartCoroutine(FadeGameOverUI(true));
            gameOverText.text = $"Game Over!\nPlayer {(winner == GameManager.Instance.Player1 ? "1" : "2")} Wins!";

            // Hide damage resolver
            if (damageResolver != null) {
                damageResolver.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator FadeGameOverUI(bool fadeIn) {
        float targetAlpha = fadeIn ? 1f : 0f;
        float currentAlpha = gameOverCanvasGroup.alpha;
        float elapsed = 0f;

        gameOverCanvasGroup.interactable = fadeIn;
        gameOverCanvasGroup.blocksRaycasts = fadeIn;

        while (elapsed < 1f) {
            elapsed += Time.deltaTime * fadeSpeed;
            gameOverCanvasGroup.alpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsed);
            yield return null;
        }

        gameOverCanvasGroup.alpha = targetAlpha;

        // If we're fading out and fully transparent, we can disable the panel
        if (!fadeIn && gameOverCanvasGroup.alpha <= 0f) {
            gameOverPanel.SetActive(false);
        }
    }

    private void UpdateResolutionUI() {
        if (damageResolver != null) {
            damageResolver.UpdateResolutionState();
        }
    }

    private void HandleRestart() {
        StartCoroutine(RestartSequence());
    }

    private IEnumerator RestartSequence() {
        // Fade out game over UI
        yield return StartCoroutine(FadeGameOverUI(false));

        // Reset game
        GameManager.Instance.RestartGame();

        // Show damage resolver
        if (damageResolver != null) {
            damageResolver.gameObject.SetActive(true);
        }
    }

    // Optional: Add this method if you need to hide the game over UI immediately
    public void HideGameOverUI() {
        if (gameOverCanvasGroup != null) {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
            gameOverPanel.SetActive(false);
        }
    }
}