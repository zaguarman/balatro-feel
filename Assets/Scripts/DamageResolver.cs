using TMPro;
using UnityEngine.UI;

public class DamageResolver {
    private static DamageResolver instance;
    public static DamageResolver Instance => instance;

    private readonly Button resolveActionsButton;
    private readonly TextMeshProUGUI pendingDamageText;
    private readonly GameContext gameContext;
    private readonly GameManager gameManager;
    private bool isResolving;

    public DamageResolver(Button resolveButton, TextMeshProUGUI damageText, GameManager manager) {
        if (instance != null) return;

        instance = this;
        resolveActionsButton = resolveButton;
        pendingDamageText = damageText;
        gameManager = manager;
        gameContext = manager?.GameContext;

        if (resolveActionsButton != null) {
            resolveActionsButton.onClick.AddListener(ResolveDamage);
        }

        GameMediator.Instance?.RegisterDamageResolver(this);
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
        gameManager?.NotifyGameStateChanged();
        isResolving = false;
        UpdateResolutionState();
    }

    public void Cleanup() {
        GameMediator.Instance?.UnregisterDamageResolver(this);

        if (resolveActionsButton != null) {
            resolveActionsButton.onClick.RemoveListener(ResolveDamage);
        }

        if (instance == this) {
            instance = null;
        }
    }
}