using UnityEngine;

public abstract class UIComponent : MonoBehaviour {
    protected GameMediator Mediator => GameMediator.Instance;
    protected GameReferences References => GameReferences.Instance;

    protected virtual void OnEnable() {
        // Only register events if the system is fully initialized
        // TODO: Check what component is the last to initialize and add it here
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            RegisterEvents();
            UpdateUI();
        }
    }

    protected virtual void OnDisable() {
        UnregisterEvents();
    }

    protected abstract void RegisterEvents();
    protected abstract void UnregisterEvents();
    public abstract void UpdateUI();
}