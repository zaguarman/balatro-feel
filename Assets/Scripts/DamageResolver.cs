using TMPro;
using UnityEngine.UI;

public class DamageResolver {
    private static DamageResolver instance;
    public static DamageResolver Instance => instance;

    private readonly Button resolveActionsButton;
    private readonly TextMeshProUGUI pendingDamageText;
    private GameContext gameContext;
    private GameManager gameManager;
    private bool isResolving = false;

    public DamageResolver() {
        if (instance != null) {
            return;
        }
        instance = this;

        var references = GameReferences.Instance;
        resolveActionsButton = references.GetResolveActionsButton();
        pendingDamageText = references.GetPendingDamageText();

        gameManager = GameManager.Instance;
        if (gameManager != null) {
            gameContext = gameManager.GameContext;
        }

        SetupButton();
        if (GameMediator.Instance != null) {
            GameMediator.Instance.RegisterDamageResolver(this);
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
        if (gameContext == null || isResolving) return;
        isResolving = true;
        gameContext.ResolveActions();
        if (gameManager != null) {
            gameManager.NotifyGameStateChanged();
        }
        isResolving = false;
        UpdateResolutionState();
    }

    public void Cleanup() {
        if (GameMediator.Instance != null) {
            GameMediator.Instance.UnregisterDamageResolver(this);
        }
        if (resolveActionsButton != null) {
            resolveActionsButton.onClick.RemoveListener(ResolveDamage);
        }
        if (instance == this) {
            instance = null;
        }
    }
}