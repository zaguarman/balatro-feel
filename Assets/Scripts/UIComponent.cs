using UnityEngine;

public abstract class UIComponent : MonoBehaviour {
    protected GameMediator Mediator => GameMediator.Instance;
    protected GameReferences References => GameReferences.Instance;

    protected virtual void OnEnable() {
        RegisterEvents();
        UpdateUI();
    }

    protected virtual void OnDisable() {
        UnregisterEvents();
    }

    protected abstract void RegisterEvents();
    protected abstract void UnregisterEvents();
    public abstract void UpdateUI();
}