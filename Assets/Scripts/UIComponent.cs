using UnityEngine;

public abstract class UIComponent : InitializableComponent {
    protected GameMediator gameMediator => GameMediator.Instance;
    protected GameReferences gameReferences => GameReferences.Instance;

    protected override void Awake() {
        base.Awake();
        // Any additional Awake logic for UI components
    }

    public override void Initialize() {
        if (IsInitialized) return;

        // Ensure dependencies are initialized
        if (!InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            Debug.LogWarning($"{GetType().Name}: GameMediator not initialized yet");
            return;
        }

        RegisterEvents();
        UpdateUI();
        base.Initialize();
    }

    protected virtual void OnEnable() {
        if (IsInitialized) {
            RegisterEvents();
            UpdateUI();
        }
    }

    protected virtual void OnDisable() {
        if (IsInitialized) {
            UnregisterEvents();
        }
    }

    protected abstract void RegisterEvents();
    protected abstract void UnregisterEvents();
    public abstract void UpdateUI();
}