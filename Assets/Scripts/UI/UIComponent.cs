using UnityEngine;
using UnityEngine.Events;

public abstract class UIComponent : InitializableComponent {
    protected GameMediator gameMediator => GameMediator.Instance;
    protected GameReferences gameReferences => GameReferences.Instance;
    protected GameManager gameManager => GameManager.Instance;

    private bool hasBeenDestroyed = false;

    public UnityEvent onInitialized = new UnityEvent();

    public IPlayer Player { get; private set; }

    protected override void Awake() {
        base.Awake();
    }

    public virtual void Initialize(IPlayer player = null) {
        if (Player == null) {
            Player = player;
        }

        if (IsInitialized) return;

        // Ensure dependencies are initialized
        if (!InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            Debug.LogWarning($"{GetType().Name}: GameMediator not initialized yet");
            return;
        }

        RegisterEvents();
        base.Initialize();

        // Fire the UnityEvent when initialization is complete
        onInitialized.Invoke();
    }

    protected virtual void OnEnable() {
        if (IsInitialized && !hasBeenDestroyed) {
            RegisterEvents();
        }
    }

    protected virtual void OnDisable() {
        if (IsInitialized) {
            UnregisterEvents();
        }
    }

    protected override void OnDestroy() {
        if (IsInitialized) {
            onInitialized.RemoveAllListeners();
            UnregisterEvents();
            CleanupComponent();
        }
        base.OnDestroy();
    }

    protected virtual void CleanupComponent() {
        // Override in derived classes to perform specific cleanup
    }

    protected abstract void RegisterEvents();
    protected abstract void UnregisterEvents();
    public abstract void UpdateUI(IPlayer player = null);
}