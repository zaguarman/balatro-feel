using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

[Serializable] public class PlayerDamagedEvent : UnityEvent<IPlayer, int> { }
[Serializable] public class CreatureDamagedEvent : UnityEvent<ICreature, int> { }
[Serializable] public class CreatureDiedEvent : UnityEvent<ICreature> { }
[Serializable] public class GameOverEvent : UnityEvent<IPlayer> { }

public interface IGameMediator {
    UnityEvent OnGameStateChanged { get; }
    PlayerDamagedEvent OnPlayerDamaged { get; }
    CreatureDamagedEvent OnCreatureDamaged { get; }
    CreatureDiedEvent OnCreatureDied { get; }
    GameOverEvent OnGameOver { get; }

    void Initialize();
    void RegisterPlayer(IPlayer player);
    void UnregisterPlayer(IPlayer player);
    void RegisterUI(GameUI ui);
    void UnregisterUI(GameUI ui);
    void RegisterDamageResolver(DamageResolver resolver);
    void UnregisterDamageResolver(DamageResolver resolver);
    void NotifyGameStateChanged();
    void NotifyPlayerDamaged(IPlayer player, int damage);
    void NotifyCreatureDamaged(ICreature creature, int damage);
    void NotifyCreatureDied(ICreature creature);
    void NotifyGameOver(IPlayer winner);
    void Cleanup();
}

public class GameMediator : Singleton<GameMediator>, IGameMediator {
    [SerializeField] private UnityEvent onGameStateChanged = new UnityEvent();
    [SerializeField] private PlayerDamagedEvent onPlayerDamaged = new PlayerDamagedEvent();
    [SerializeField] private CreatureDamagedEvent onCreatureDamaged = new CreatureDamagedEvent();
    [SerializeField] private CreatureDiedEvent onCreatureDied = new CreatureDiedEvent();
    [SerializeField] private GameOverEvent onGameOver = new GameOverEvent();

    public UnityEvent OnGameStateChanged => onGameStateChanged;
    public PlayerDamagedEvent OnPlayerDamaged => onPlayerDamaged;
    public CreatureDamagedEvent OnCreatureDamaged => onCreatureDamaged;
    public CreatureDiedEvent OnCreatureDied => onCreatureDied;
    public GameOverEvent OnGameOver => onGameOver;

    private readonly List<IPlayer> players = new List<IPlayer>();
    private GameUI gameUI;
    private DamageResolver damageResolver;
    private bool isInitialized;

    protected override void Awake() {
        base.Awake();
        // Keep any specific Awake logic if needed
    }

    public void Initialize() {
        if (isInitialized) {
            Debug.LogWarning("GameMediator already initialized");
            return;
        }
        ClearRegistrations();
        isInitialized = true;
    }

    private bool ValidateState(string operation) {
        if (!isInitialized) {
            Debug.LogError($"GameMediator not initialized during {operation}");
            return false;
        }
        return true;
    }

    public void RegisterPlayer(IPlayer player) {
        if (!ValidateState("RegisterPlayer") || player == null) return;

        if (!players.Contains(player)) {
            players.Add(player);
            player.OnDamaged.AddListener((damage) => NotifyPlayerDamaged(player, damage));
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (!isInitialized || player == null) return;
        players.Remove(player);
    }

    public void RegisterUI(GameUI ui) {
        if (!ValidateState("RegisterUI") || ui == null) return;

        gameUI = ui;
        onGameStateChanged.AddListener(ui.UpdateUI);
        onPlayerDamaged.AddListener((player, damage) => ui.UpdatePlayerHealth(player));
    }

    public void UnregisterUI(GameUI ui) {
        if (!isInitialized || gameUI != ui) return;

        onGameStateChanged.RemoveListener(ui.UpdateUI);
        onPlayerDamaged.RemoveAllListeners();
        gameUI = null;
    }

    public void RegisterDamageResolver(DamageResolver resolver) {
        if (!ValidateState("RegisterDamageResolver") || resolver == null) return;

        damageResolver = resolver;
        onGameStateChanged.AddListener(resolver.UpdateResolutionState);
    }

    public void UnregisterDamageResolver(DamageResolver resolver) {
        if (!isInitialized || damageResolver != resolver) return;

        onGameStateChanged.RemoveListener(resolver.UpdateResolutionState);
        damageResolver = null;
    }

    public void NotifyGameStateChanged() {
        if (!isInitialized) return;
        onGameStateChanged.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        if (!isInitialized || player == null) return;

        onPlayerDamaged.Invoke(player, damage);
        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        if (!isInitialized || creature == null) return;

        onCreatureDamaged.Invoke(creature, damage);
        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        if (!isInitialized || creature == null) return;
        onCreatureDied.Invoke(creature);
    }

    public void NotifyGameOver(IPlayer winner) {
        if (!isInitialized || winner == null) return;
        onGameOver.Invoke(winner);
    }

    private void ClearRegistrations() {
        players.Clear();
        gameUI = null;
        damageResolver = null;

        onGameStateChanged.RemoveAllListeners();
        onPlayerDamaged.RemoveAllListeners();
        onCreatureDamaged.RemoveAllListeners();
        onCreatureDied.RemoveAllListeners();
        onGameOver.RemoveAllListeners();
    }

    public void Cleanup() {
        if (!isInitialized) return;

        ClearRegistrations();
        isInitialized = false;
    }

    protected override void OnDestroy() {
        if (instance == this) {
            Cleanup();
            instance = null;
        }
    }
}