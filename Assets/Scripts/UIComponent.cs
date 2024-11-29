using UnityEngine;

public abstract class UIComponent : MonoBehaviour {
    protected GameMediator Mediator => GameMediator.Instance;
    protected GameReferences References => GameReferences.Instance;
    protected GameMediator gameMediator;
    protected bool isInitialized;

    protected virtual void OnEnable() {
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            InitializeEvents();
            UpdateUI();
        }
    }

    protected virtual void OnDisable() {
        UnregisterEvents();
    }

    protected virtual void OnDestroy() {
        UnregisterEvents();
    }

    protected virtual void InitializeEvents() {
        gameMediator = GameMediator.Instance;
        if (gameMediator != null) {
            RegisterEvents();
        }
    }

    protected abstract void RegisterEvents();
    protected abstract void UnregisterEvents();
    public abstract void UpdateUI();
}