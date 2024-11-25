using System.Collections.Generic;
using UnityEngine;

public class GameMediator : Singleton<GameMediator> {
    private GameEvents gameEvents;
    private readonly List<IPlayer> players = new List<IPlayer>();
    private GameUI gameUI;
    private bool isInitialized;

    protected override void Awake() {
        base.Awake();
        Initialize();
    }

    public void Initialize() {
        if (!isInitialized) {
            ClearRegistrations();
            isInitialized = true;
        }
    }

    private bool ValidateState(string operation) {
        if (!isInitialized) {
            Debug.LogError($"GameMediator: Attempted {operation} before initialization");
            Initialize();
        }
        return isInitialized;
    }

    // Event accessors
    public GameEvents.GameStateChangedEvent OnGameStateChanged => gameEvents.OnGameStateChanged;
    public GameEvents.PlayerDamagedEvent OnPlayerDamaged => gameEvents.OnPlayerDamaged;
    public GameEvents.CreatureDamagedEvent OnCreatureDamaged => gameEvents.OnCreatureDamaged;
    public GameEvents.CreatureDiedEvent OnCreatureDied => gameEvents.OnCreatureDied;
    public GameEvents.GameOverEvent OnGameOver => gameEvents.OnGameOver;
    public GameEvents.GameInitializedEvent OnGameInitialized => gameEvents.OnGameInitialized;

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
        Debug.Log($"Unregistered player: {player.TargetId}");
    }

    public void RegisterUI(GameUI ui) {
        if (!ValidateState("RegisterUI") || ui == null) return;
        gameUI = ui;
        Debug.Log("Registered GameUI");
    }

    public void UnregisterUI(GameUI ui) {
        if (gameUI == ui) {
            gameUI = null;
            Debug.Log("Unregistered GameUI");
        }
    }

    // Notification methods
    public void NotifyGameInitialized() {
        if (!ValidateState("NotifyGameInitialized")) return;
        gameEvents.OnGameInitialized.Invoke();
        Debug.Log("Game initialization completed");
    }

    public void NotifyGameStateChanged() {
        if (!ValidateState("NotifyGameStateChanged")) return;
        gameEvents.OnGameStateChanged.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        if (!ValidateState("NotifyPlayerDamaged") || player == null) return;

        gameEvents.OnPlayerDamaged.Invoke(player, damage);
        Debug.Log($"Player {player.TargetId} damaged for {damage}");

        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        if (!ValidateState("NotifyCreatureDamaged") || creature == null) return;

        gameEvents.OnCreatureDamaged.Invoke(creature, damage);
        Debug.Log($"Creature {creature.Name} damaged for {damage}");

        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        if (!ValidateState("NotifyCreatureDied") || creature == null) return;

        gameEvents.OnCreatureDied.Invoke(creature);
        Debug.Log($"Creature {creature.Name} died");
        NotifyGameStateChanged();
    }

    public void NotifyGameOver(IPlayer winner) {
        if (!ValidateState("NotifyGameOver") || winner == null) return;

        gameEvents.OnGameOver.Invoke(winner);
        Debug.Log($"Game over - Winner: {winner.TargetId}");
    }

    private void ClearRegistrations() {
        players.Clear();
        gameUI = null;
    }

    public void Cleanup() {
        ClearRegistrations();
        isInitialized = false;
    }

    protected override void OnDestroy() {
        if (instance == this) {
            Cleanup();
        }
        base.OnDestroy();
    }
}