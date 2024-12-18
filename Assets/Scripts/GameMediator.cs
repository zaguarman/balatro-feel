using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using static DebugLogger;

public class GameMediator : Singleton<GameMediator> {
    private readonly List<IGameObserver> observers = new List<IGameObserver>();
    private readonly HashSet<IPlayer> registeredPlayers = new HashSet<IPlayer>();
    private bool gameInitialized = false;
    private readonly GameEvents events = new GameEvents();

    public void AddObserver(IGameObserver observer) {
        if (!observers.Contains(observer)) {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IGameObserver observer) {
        observers.Remove(observer);
    }

    public void NotifyGameStateChanged() {
        ValidateInitialization();
        events.GameStateChanged.Invoke();
        foreach (var observer in observers) {
            observer.OnGameStateChanged();
        }
    }

    public void NotifyGameInitialized() {
        ValidateInitialization();
        if (!gameInitialized) {
            gameInitialized = true;
            Log("Game initialized", LogTag.Initialization);
            events.GameInitialized.Invoke();
            foreach (var observer in observers) {
                observer.OnGameInitialized();
            }
        }
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        ValidateInitialization();
        if (player == null) throw new System.ArgumentNullException(nameof(player));

        events.PlayerDamaged.Invoke(player, damage);
        foreach (var observer in observers) {
            observer.OnPlayerDamaged(player, damage);
        }
        Log($"{damage} damage to {(player.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Players | LogTag.Combat);

        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        events.CreatureDamaged.Invoke(creature, damage);
        foreach (var observer in observers) {
            observer.OnCreatureDamaged(creature, damage);
        }
        Log($"{damage} damage to {creature.Name}", LogTag.Creatures | LogTag.Combat);

        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        events.CreatureDied.Invoke(creature);
        foreach (var observer in observers) {
            observer.OnCreatureDied(creature);
        }
        Log($"Creature died: {creature.Name}", LogTag.Creatures);
        NotifyGameStateChanged();
    }

    public void NotifyGameOver(IPlayer winner) {
        ValidateInitialization();
        if (winner == null) throw new System.ArgumentNullException(nameof(winner));

        Log($"Game over: {(winner.IsPlayer1() ? "Player 1" : "Player 2")} wins", LogTag.Players);
        events.GameOver.Invoke(winner);
        foreach (var observer in observers) {
            observer.OnGameOver(winner);
        }
    }

    public void NotifyCreaturePreSummon(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        events.CreaturePreSummon.Invoke(creature);
        foreach (var observer in observers) {
            observer.OnCreaturePreSummon(creature);
        }
        Log($"Creature pre-summon: {creature.Name}", LogTag.Creatures);
    }

    public void NotifyCreatureSummoned(ICreature creature, IPlayer owner) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));
        if (owner == null) throw new System.ArgumentNullException(nameof(owner));

        events.CreatureSummoned.Invoke(creature, owner);
        foreach (var observer in observers) {
            observer.OnCreatureSummoned(creature, owner);
        }
        Log($"Creature summoned: {creature.Name} by {(owner.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Creatures);
        NotifyGameStateChanged();
    }

    protected override void OnDestroy() {
        if (this == Instance) {
            foreach (var observer in observers.ToList()) {
                RemoveObserver(observer);
            }
            events.ClearAllListeners();
            registeredPlayers.Clear();
            observers.Clear();
        }
        base.OnDestroy();
    }

    private void ValidateInitialization() {
        if (!IsInitialized) {
            throw new System.InvalidOperationException("GameMediator is not initialized");
        }
    }

    #region Event Registration Methods
    public void AddGameStateChangedListener(UnityAction listener) {
        ValidateInitialization();
        events.GameStateChanged.AddListener(listener);
    }

    public void RemoveGameStateChangedListener(UnityAction listener) {
        events.GameStateChanged.RemoveListener(listener);
    }

    public void AddPlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        ValidateInitialization();
        events.PlayerDamaged.AddListener(listener);
    }

    public void RemovePlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        events.PlayerDamaged.RemoveListener(listener);
    }

    public void AddCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        ValidateInitialization();
        events.CreatureDamaged.AddListener(listener);
    }

    public void RemoveCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        events.CreatureDamaged.RemoveListener(listener);
    }

    public void AddCreatureDiedListener(UnityAction<ICreature> listener) {
        ValidateInitialization();
        events.CreatureDied.AddListener(listener);
    }

    public void RemoveCreatureDiedListener(UnityAction<ICreature> listener) {
        events.CreatureDied.RemoveListener(listener);
    }

    public void AddGameInitializedListener(UnityAction listener) {
        ValidateInitialization();
        events.GameInitialized.AddListener(listener);
    }

    public void RemoveGameInitializedListener(UnityAction listener) {
        events.GameInitialized.RemoveListener(listener);
    }
    #endregion

    #region Player Registration
    public void RegisterPlayer(IPlayer player) {
        ValidateInitialization();
        if (player == null) throw new System.ArgumentNullException(nameof(player));

        if (registeredPlayers.Add(player)) {
            player.OnDamaged.AddListener((damage) => NotifyPlayerDamaged(player, damage));
            Log($"Player registered: {(player.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Players);
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (player == null) return;
        if (registeredPlayers.Remove(player)) {
            Log($"Player unregistered: {(player.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Players);
        }
    }
    #endregion
}