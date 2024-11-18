using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageResolver : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Button resolveButton;
    [SerializeField] private TextMeshProUGUI pendingDamageText;

    [Header("Resolution Settings")]
    [SerializeField] private float resolutionDelay = 0.5f;
    [SerializeField] private bool autoResolveWhenEmpty = true;

    private GameContext gameContext;
    private GameManager gameManager;
    private bool isResolving = false;

    public void Awake() {
        gameManager = GameManager.Instance;
        if (gameManager != null) {
            gameContext = gameManager.GameContext;
        }

        SetupButton();
    }

    private void SetupButton() {
        if (resolveButton != null) {
            resolveButton.onClick.AddListener(ResolveDamage);
        }
    }

    public void UpdateResolutionState() {
        if (gameContext == null || pendingDamageText == null) return;

        int pendingActions = gameContext.GetPendingActionsCount();
        pendingDamageText.text = $"Pending Actions: {pendingActions}";

        if (resolveButton != null) {
            resolveButton.interactable = pendingActions > 0 && !isResolving && !gameManager.CheckGameEnd(); // Disable when game is over
        }

        if (autoResolveWhenEmpty && pendingActions == 0) {
            resolveButton.interactable = false;
        }
    }

    public void ResolveDamage() {
        if (gameContext != null && !isResolving && !gameManager.CheckGameEnd()) {
            StartCoroutine(ResolveDamageSequence());
        }
    }

    private IEnumerator ResolveDamageSequence() {
        isResolving = true;
        resolveButton.interactable = false;

        gameContext.ResolveActions();

        yield return new WaitForSeconds(resolutionDelay);

        if (gameManager != null) {
            gameManager.NotifyGameStateChanged();
        }

        isResolving = false;
        UpdateResolutionState();
    }

    public void OnEnable() {
        UpdateResolutionState();
    }
}