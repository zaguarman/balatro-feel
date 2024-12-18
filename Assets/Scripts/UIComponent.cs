using UnityEngine;

public abstract class UIComponent : InitializableComponent, IGameObserver {
    protected GameMediator gameMediator => GameMediator.Instance;
    protected GameReferences gameReferences => GameReferences.Instance;
    private bool hasBeenDestroyed = false;

    public override void Initialize() {
        if (IsInitialized) return;

        if (!InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            Debug.LogWarning($"{GetType().Name}: GameMediator not initialized yet");
            return;
        }

        gameMediator.AddObserver(this);
        UpdateUI();
        base.Initialize();
    }

    protected virtual void OnEnable() {
        if (IsInitialized && !hasBeenDestroyed) {
            UpdateUI();
        }
    }

    protected virtual void OnDestroy() {
        hasBeenDestroyed = true;
        if (IsInitialized && gameMediator != null) {
            gameMediator.RemoveObserver(this);
            CleanupComponent();
        }
    }

    protected virtual void CleanupComponent() { }
    public abstract void UpdateUI();

    // IGameObserver default implementations
    public virtual void OnGameStateChanged() { }
    public virtual void OnGameInitialized() { }
    public virtual void OnPlayerDamaged(IPlayer player, int damage) { }
    public virtual void OnCreatureDamaged(ICreature creature, int damage) { }
    public virtual void OnCreatureDied(ICreature creature) { }
    public virtual void OnGameOver(IPlayer winner) { }
    public virtual void OnCreaturePreSummon(ICreature creature) { }
    public virtual void OnCreatureSummoned(ICreature creature, IPlayer owner) { }
}