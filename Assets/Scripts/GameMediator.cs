using UnityEngine;
using System;
using System.Collections.Generic;

public interface IGameMediator {
    void Initialize();
    void RegisterPlayer(IPlayer player);
    void RegisterUI(GameUI ui);
    void RegisterDamageResolver(DamageResolver resolver);
    void NotifyGameStateChanged();
    void NotifyPlayerDamaged(IPlayer player, int damage);
    void NotifyCreatureDamaged(ICreature creature, int damage);
    void NotifyCreatureDied(ICreature creature);
    void NotifyGameOver(IPlayer winner);
    void Cleanup();
}

public class GameMediator : MonoBehaviour, IGameMediator {
    private static GameMediator instance;
    public static GameMediator Instance {
        get {
            if (instance == null) {
                var go = new GameObject("GameMediator");
                instance = go.AddComponent<GameMediator>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private readonly List<IPlayer> players = new List<IPlayer>();
    private GameUI gameUI;
    private DamageResolver damageResolver;

    public event Action onGameStateChanged;
    public event Action<IPlayer, int> onPlayerDamaged;
    public event Action<ICreature, int> onCreatureDamaged;
    public event Action<ICreature> onCreatureDied;
    public event Action<IPlayer> onGameOver;

    private bool isInitialized;

    public void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize() {
        if (isInitialized) {
            Debug.LogWarning("GameMediator already initialized");
            return;
        }

        players.Clear();
        gameUI = null;
        damageResolver = null;
        isInitialized = true;
        Debug.Log("GameMediator initialized");
    }

    public void RegisterPlayer(IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameMediator not initialized");
            return;
        }

        if (!players.Contains(player)) {
            players.Add(player);
            player.OnDamaged += (damage) => NotifyPlayerDamaged(player, damage);
            Debug.Log($"Registered player with ID: {player.TargetId}");
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (!isInitialized) return;

        if (players.Contains(player)) {
            players.Remove(player);
            Debug.Log($"Unregistered player with ID: {player.TargetId}");
        }
    }

    public void RegisterUI(GameUI ui) {
        if (!isInitialized) {
            Debug.LogError("GameMediator not initialized");
            return;
        }

        if (ui != null) {
            gameUI = ui;
            onGameStateChanged += ui.UpdateUI;
            onPlayerDamaged += (player, damage) => ui.UpdatePlayerHealth(player);
            Debug.Log("Registered GameUI");
        }
    }

    public void UnregisterUI(GameUI ui) {
        if (!isInitialized) return;

        if (gameUI == ui) {
            onGameStateChanged -= ui.UpdateUI;
            gameUI = null;
            Debug.Log("Unregistered GameUI");
        }
    }

    public void RegisterDamageResolver(DamageResolver resolver) {
        if (!isInitialized) {
            Debug.LogError("GameMediator not initialized");
            return;
        }

        if (resolver != null) {
            damageResolver = resolver;
            onGameStateChanged += resolver.UpdateResolutionState;
            Debug.Log("Registered DamageResolver");
        }
    }

    public void UnregisterDamageResolver(DamageResolver resolver) {
        if (!isInitialized) return;

        if (damageResolver == resolver) {
            onGameStateChanged -= resolver.UpdateResolutionState;
            damageResolver = null;
            Debug.Log("Unregistered DamageResolver");
        }
    }

    public void NotifyGameStateChanged() {
        if (!isInitialized) return;

        Debug.Log("Game state changed notification");
        onGameStateChanged?.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        if (!isInitialized) return;

        Debug.Log($"Player {player.TargetId} damaged for {damage}");
        onPlayerDamaged?.Invoke(player, damage);
        CheckGameOver(player);
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        if (!isInitialized) return;

        Debug.Log($"Creature {creature.Name} damaged for {damage}");
        onCreatureDamaged?.Invoke(creature, damage);
        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        if (!isInitialized) return;

        Debug.Log($"Creature {creature.Name} died");
        onCreatureDied?.Invoke(creature);
    }

    public void NotifyGameOver(IPlayer winner) {
        if (!isInitialized) return;

        Debug.Log($"Game over - Winner: {winner.TargetId}");
        onGameOver?.Invoke(winner);
    }

    private void CheckGameOver(IPlayer player) {
        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void Cleanup() {
        if (!isInitialized) return;

        players.Clear();
        if (gameUI != null) {
            UnregisterUI(gameUI);
        }
        if (damageResolver != null) {
            UnregisterDamageResolver(damageResolver);
        }

        onGameStateChanged = null;
        onPlayerDamaged = null;
        onCreatureDamaged = null;
        onCreatureDied = null;
        onGameOver = null;

        isInitialized = false;
        Debug.Log("GameMediator cleaned up");
    }

    public void OnDestroy() {
        if (instance == this) {
            Cleanup();
            instance = null;
        }
    }

    public void Reset() {
        Cleanup();
        Initialize();
    }
}