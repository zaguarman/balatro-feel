using UnityEngine;
using System.Collections.Generic;

public interface IGameMediator {
    GameEvents.GameStateChangedEvent OnGameStateChanged { get; }
    GameEvents.PlayerDamagedEvent OnPlayerDamaged { get; }
    GameEvents.CreatureDamagedEvent OnCreatureDamaged { get; }
    GameEvents.CreatureDiedEvent OnCreatureDied { get; }
    GameEvents.GameOverEvent OnGameOver { get; }

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
    private GameEvents gameEvents;

    protected override void Awake() {
        base.Awake();
        gameEvents = GameEvents.Instance;
    }

    public GameEvents.GameStateChangedEvent OnGameStateChanged => gameEvents.OnGameStateChanged;
    public GameEvents.PlayerDamagedEvent OnPlayerDamaged => gameEvents.OnPlayerDamaged;
    public GameEvents.CreatureDamagedEvent OnCreatureDamaged => gameEvents.OnCreatureDamaged;
    public GameEvents.CreatureDiedEvent OnCreatureDied => gameEvents.OnCreatureDied;
    public GameEvents.GameOverEvent OnGameOver => gameEvents.OnGameOver;

    private readonly List<IPlayer> players = new List<IPlayer>();
    private GameUI gameUI;
    private DamageResolver damageResolver;
    private bool isInitialized;

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
        // The UI components will now register their own event listeners via UIComponent base class
    }

    public void UnregisterUI(GameUI ui) {
        if (!isInitialized || gameUI != ui) return;
        gameUI = null;
    }

    public void RegisterDamageResolver(DamageResolver resolver) {
        if (!ValidateState("RegisterDamageResolver") || resolver == null) return;
        damageResolver = resolver;
    }

    public void UnregisterDamageResolver(DamageResolver resolver) {
        if (!isInitialized || damageResolver != resolver) return;
        damageResolver = null;
    }

    public void NotifyGameStateChanged() => gameEvents.NotifyGameStateChanged();
    public void NotifyPlayerDamaged(IPlayer player, int damage) => gameEvents.NotifyPlayerDamaged(player, damage);
    public void NotifyCreatureDamaged(ICreature creature, int damage) => gameEvents.NotifyCreatureDamaged(creature, damage);
    public void NotifyCreatureDied(ICreature creature) => gameEvents.NotifyCreatureDied(creature);
    public void NotifyGameOver(IPlayer winner) => gameEvents.NotifyGameOver(winner);

    private void ClearRegistrations() {
        players.Clear();
        gameUI = null;
        damageResolver = null;
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