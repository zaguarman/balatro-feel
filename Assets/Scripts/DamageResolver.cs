using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageResolver : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Button resolveActionsButton;
    [SerializeField] private TextMeshProUGUI pendingDamageText;

    [Header("Resolution Settings")]
    [SerializeField] private float resolutionDelay = 0.5f;

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

    private void OnEnable() {
        if (GameMediator.Instance != null) {
            GameMediator.Instance.RegisterDamageResolver(this);
        }
        UpdateResolutionState();
    }

    private void OnDisable() {
        if (GameMediator.Instance != null) {
            GameMediator.Instance.UnregisterDamageResolver(this);
        }
    }

    private void SetupButton() {
        if (resolveActionsButton != null) {
            resolveActionsButton.onClick.AddListener(ResolveDamage);
        }
    }

    public void UpdateResolutionState() {
        if (gameContext == null || pendingDamageText == null) return;

        int pendingActions = gameContext.GetPendingActionsCount();
        pendingDamageText.text = $"Pending Actions: {pendingActions}";
    }

    public void ResolveDamage() {
        if (gameContext != null && !isResolving) {
            StartCoroutine(ResolveDamageSequence());
        }
    }

    private IEnumerator ResolveDamageSequence() {
        isResolving = true;

        gameContext.ResolveActions();

        yield return new WaitForSeconds(resolutionDelay);

        if (gameManager != null) {
            gameManager.NotifyGameStateChanged();
        }

        isResolving = false;
        UpdateResolutionState();
    }
}