using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameMediator : Singleton<GameMediator>, IInitializable {
    public bool IsInitialized { get; private set; }

    private readonly List<IPlayer> players = new List<IPlayer>();
    private GameUI gameUI;
    private InitializationManager initManager;

    // Event definitions
    [Serializable] private class PlayerDamagedEvent : UnityEvent<IPlayer, int> { }
    [Serializable] private class CreatureDamagedEvent : UnityEvent<ICreature, int> { }
    [Serializable] private class CreatureDiedEvent : UnityEvent<ICreature> { }
    [Serializable] private class GameOverEvent : UnityEvent<IPlayer> { }
    [Serializable] private class GameStateChangedEvent : UnityEvent { }
    [Serializable] private class GameInitializedEvent : UnityEvent { }

    // Private event instances
    private readonly PlayerDamagedEvent onPlayerDamaged = new PlayerDamagedEvent();
    private readonly CreatureDamagedEvent onCreatureDamaged = new CreatureDamagedEvent();
    private readonly CreatureDiedEvent onCreatureDied = new CreatureDiedEvent();
    private readonly GameOverEvent onGameOver = new GameOverEvent();
    private readonly GameStateChangedEvent onGameStateChanged = new GameStateChangedEvent();
    private readonly GameInitializedEvent onGameInitialized = new GameInitializedEvent();

    protected override void Awake() {
        base.Awake();
        initManager = InitializationManager.Instance;
        initManager.RegisterComponent(this);
        Initialize();
    }

    public void Initialize() {
        if (IsInitialized) return;

        ClearRegistrations();
        IsInitialized = true;
        initManager.MarkComponentInitialized(this);
    }

    private bool ValidateState(string operation) {
        if (!IsInitialized) {
            Initialize();
        }
        return IsInitialized;
    }

    // Event subscription methods
    public void AddGameStateChangedListener(UnityAction listener) {
        if (!ValidateState("AddGameStateChangedListener") || listener == null) return;
        onGameStateChanged.AddListener(listener);
    }

    public void RemoveGameStateChangedListener(UnityAction listener) {
        if (listener == null) return;
        onGameStateChanged.RemoveListener(listener);
    }

    public void AddPlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        if (!ValidateState("AddPlayerDamagedListener") || listener == null) return;
        onPlayerDamaged.AddListener(listener);
    }

    public void RemovePlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        if (listener == null) return;
        onPlayerDamaged.RemoveListener(listener);
    }

    public void AddCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        if (!ValidateState("AddCreatureDamagedListener") || listener == null) return;
        onCreatureDamaged.AddListener(listener);
    }

    public void RemoveCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        if (listener == null) return;
        onCreatureDamaged.RemoveListener(listener);
    }

    public void AddCreatureDiedListener(UnityAction<ICreature> listener) {
        if (!ValidateState("AddCreatureDiedListener") || listener == null) return;
        onCreatureDied.AddListener(listener);
    }

    public void RemoveCreatureDiedListener(UnityAction<ICreature> listener) {
        if (listener == null) return;
        onCreatureDied.RemoveListener(listener);
    }

    public void AddGameOverListener(UnityAction<IPlayer> listener) {
        if (!ValidateState("AddGameOverListener") || listener == null) return;
        onGameOver.AddListener(listener);
    }

    public void RemoveGameOverListener(UnityAction<IPlayer> listener) {
        if (listener == null) return;
        onGameOver.RemoveListener(listener);
    }

    public void AddGameInitializedListener(UnityAction listener) {
        if (!ValidateState("AddGameInitializedListener") || listener == null) return;
        onGameInitialized.AddListener(listener);
    }

    public void RemoveGameInitializedListener(UnityAction listener) {
        if (listener == null) return;
        onGameInitialized.RemoveListener(listener);
    }

    // Registration methods
    public void RegisterPlayer(IPlayer player) {
        if (!ValidateState("RegisterPlayer") || player == null) return;

        if (!players.Contains(player)) {
            players.Add(player);
            player.OnDamaged.AddListener((damage) => NotifyPlayerDamaged(player, damage));
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (player == null) return;
        players.Remove(player);
    }

    public void RegisterUI(GameUI ui) {
        if (!ValidateState("RegisterUI") || ui == null) return;
        gameUI = ui;
    }

    public void UnregisterUI(GameUI ui) {
        if (gameUI == ui) {
            gameUI = null;
        }
    }

    // Notification methods
    public void NotifyGameInitialized() {
        if (!ValidateState("NotifyGameInitialized")) return;
        onGameInitialized.Invoke();
    }

    public void NotifyGameStateChanged() {
        if (!ValidateState("NotifyGameStateChanged")) return;
        onGameStateChanged.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        if (!ValidateState("NotifyPlayerDamaged") || player == null) return;

        onPlayerDamaged.Invoke(player, damage);

        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        if (!ValidateState("NotifyCreatureDamaged") || creature == null) return;

        onCreatureDamaged.Invoke(creature, damage);

        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        if (!ValidateState("NotifyCreatureDied") || creature == null) return;

        onCreatureDied.Invoke(creature);
        NotifyGameStateChanged();
    }

    public void NotifyGameOver(IPlayer winner) {
        if (!ValidateState("NotifyGameOver") || winner == null) return;
        onGameOver.Invoke(winner);
    }

    private void ClearRegistrations() {
        players.Clear();
        gameUI = null;
    }

    public void Cleanup() {
        ClearRegistrations();
        IsInitialized = false;
    }

    protected override void OnDestroy() {
        if (instance == this) {
            Cleanup();
        }
        base.OnDestroy();
    }
}