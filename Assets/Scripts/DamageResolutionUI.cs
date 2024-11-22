using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageResolutionUI : UIComponent {
    [SerializeField] private Button resolveButton;
    [SerializeField] private TextMeshProUGUI pendingActionsText;

    private DamageResolver resolver;

    protected override void RegisterEvents() {
        Events.OnGameStateChanged.AddListener(UpdateUI);
    }

    protected override void UnregisterEvents() {
        Events.OnGameStateChanged.RemoveListener(UpdateUI);
    }

    protected void Start() {
        resolver = new DamageResolver(resolveButton, pendingActionsText, GameManager.Instance);
    }

    public override void UpdateUI() {
        resolver?.UpdateResolutionState();
    }

    protected void OnDestroy() {
        resolver?.Cleanup();
    }
}