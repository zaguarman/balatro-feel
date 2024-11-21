using UnityEngine;
using System;
using System.Collections.Generic;

public interface IGameMediator {
    void RegisterPlayer(IPlayer player);
    void RegisterUI(GameUI ui);
    void RegisterDamageResolver(DamageResolver resolver);
    void NotifyGameStateChanged();
    void NotifyPlayerDamaged(IPlayer player, int damage);
    void NotifyCreatureDamaged(ICreature creature, int damage);
    void NotifyCreatureDied(ICreature creature);
    void NotifyGameOver(IPlayer winner);
}

public class GameMediator : MonoBehaviour, IGameMediator {
    private static GameMediator instance;
    public static GameMediator Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameMediator>();
                if (instance == null) {
                    var go = new GameObject("GameMediator");
                    instance = go.AddComponent<GameMediator>();
                    DontDestroyOnLoad(go);
                }
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

    public void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(IPlayer player) {
        if (!players.Contains(player)) {
            players.Add(player);
            player.OnDamaged += (damage) => NotifyPlayerDamaged(player, damage);
            Debug.Log($"Registered player with ID: {player.TargetId}");
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (players.Contains(player)) {
            players.Remove(player);
            Debug.Log($"Unregistered player with ID: {player.TargetId}");
        }
    }

    public void RegisterUI(GameUI ui) {
        if (ui != null) {
            gameUI = ui;
            onGameStateChanged += ui.UpdateUI;
            onPlayerDamaged += (player, damage) => ui.UpdatePlayerHealth(player);
            Debug.Log("Registered GameUI");
        }
    }

    public void UnregisterUI(GameUI ui) {
        if (gameUI == ui) {
            onGameStateChanged -= ui.UpdateUI;
            gameUI = null;
            Debug.Log("Unregistered GameUI");
        }
    }

    public void RegisterDamageResolver(DamageResolver resolver) {
        if (resolver != null) {
            damageResolver = resolver;
            onGameStateChanged += resolver.UpdateResolutionState;
            Debug.Log("Registered DamageResolver");
        }
    }

    public void UnregisterDamageResolver(DamageResolver resolver) {
        if (damageResolver == resolver) {
            onGameStateChanged -= resolver.UpdateResolutionState;
            damageResolver = null;
            Debug.Log("Unregistered DamageResolver");
        }
    }

    public void NotifyGameStateChanged() {
        Debug.Log("Game state changed notification");
        onGameStateChanged?.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        Debug.Log($"Player {player.TargetId} damaged for {damage}");
        onPlayerDamaged?.Invoke(player, damage);
        CheckGameOver(player);
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        Debug.Log($"Creature {creature.Name} damaged for {damage}");
        onCreatureDamaged?.Invoke(creature, damage);
        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        Debug.Log($"Creature {creature.Name} died");
        onCreatureDied?.Invoke(creature);
    }

    public void NotifyGameOver(IPlayer winner) {
        Debug.Log($"Game over - Winner: {winner.TargetId}");
        onGameOver?.Invoke(winner);
    }

    private void CheckGameOver(IPlayer player) {
        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void OnDestroy() {
        if (instance == this) {
            // Clean up all registrations
            players.Clear();
            if (gameUI != null) {
                UnregisterUI(gameUI);
            }
            if (damageResolver != null) {
                UnregisterDamageResolver(damageResolver);
            }
            instance = null;
        }
    }
}