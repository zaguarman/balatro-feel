using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour {
    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;

    [Header("Game State UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;

    [Header("Resolution UI")]
    [SerializeField] private DamageResolver damageResolver;

    [Header("UI Animation")]
    [SerializeField] private float fadeSpeed = 2f;
    private CanvasGroup gameOverCanvasGroup;

    private void Awake() {
        // Add CanvasGroup component programmatically if it doesn't exist
        gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (gameOverCanvasGroup == null) {
            gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }
    }

    private void Start() {
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

        if (damageResolver != null) {
            damageResolver.gameObject.SetActive(true);
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