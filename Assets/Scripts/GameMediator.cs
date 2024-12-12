using System.Collections.Generic;
using UnityEngine.Events;
using static DebugLogger;

public class GameMediator : Singleton<GameMediator> {
    private readonly UnityEvent<IPlayer, int> onPlayerDamaged = new UnityEvent<IPlayer, int>();
    private readonly UnityEvent<ICreature, int> onCreatureDamaged = new UnityEvent<ICreature, int>();
    private readonly UnityEvent<ICreature> onCreatureDied = new UnityEvent<ICreature>();
    private readonly UnityEvent<IPlayer> onGameOver = new UnityEvent<IPlayer>();
    private readonly UnityEvent onGameStateChanged = new UnityEvent();
    private readonly UnityEvent onGameInitialized = new UnityEvent();
    private readonly UnityEvent<ICreature, IPlayer> onCreatureSummoned = new UnityEvent<ICreature, IPlayer>();
    private readonly UnityEvent<ICreature> onCreaturePreSummon = new UnityEvent<ICreature>();

    private readonly HashSet<IPlayer> registeredPlayers = new HashSet<IPlayer>();

    public override void Initialize() {
        if (IsInitialized) return;
        base.Initialize();
    }

    public void AddGameStateChangedListener(UnityAction listener) {
        ValidateInitialization();
        onGameStateChanged.AddListener(listener);
    }

    public void RemoveGameStateChangedListener(UnityAction listener) {
        onGameStateChanged.RemoveListener(listener);
    }

    public void AddPlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        ValidateInitialization();
        onPlayerDamaged.AddListener(listener);
    }

    public void RemovePlayerDamagedListener(UnityAction<IPlayer, int> listener) {
        onPlayerDamaged.RemoveListener(listener);
    }

    public void AddCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        ValidateInitialization();
        onCreatureDamaged.AddListener(listener);
    }

    public void RemoveCreatureDamagedListener(UnityAction<ICreature, int> listener) {
        onCreatureDamaged.RemoveListener(listener);
    }

    public void AddCreatureDiedListener(UnityAction<ICreature> listener) {
        ValidateInitialization();
        onCreatureDied.AddListener(listener);
    }

    public void RemoveCreatureDiedListener(UnityAction<ICreature> listener) {
        onCreatureDied.RemoveListener(listener);
    }

    public void AddGameOverListener(UnityAction<IPlayer> listener) {
        ValidateInitialization();
        onGameOver.AddListener(listener);
    }

    public void RemoveGameOverListener(UnityAction<IPlayer> listener) {
        onGameOver.RemoveListener(listener);
    }

    public void AddGameInitializedListener(UnityAction listener) {
        ValidateInitialization();
        onGameInitialized.AddListener(listener);
    }

    public void RemoveGameInitializedListener(UnityAction listener) {
        onGameInitialized.RemoveListener(listener);
    }

    public void AddCreatureSummonedListener(UnityAction<ICreature, IPlayer> listener) {
        ValidateInitialization();
        onCreatureSummoned.AddListener(listener);
    }

    public void RemoveCreatureSummonedListener(UnityAction<ICreature, IPlayer> listener) {
        onCreatureSummoned.RemoveListener(listener);
    }

    public void AddCreaturePreSummonListener(UnityAction<ICreature> listener) {
        ValidateInitialization();
        onCreaturePreSummon.AddListener(listener);
    }

    public void RemoveCreaturePreSummonListener(UnityAction<ICreature> listener) {
        onCreaturePreSummon.RemoveListener(listener);
    }

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

    public void NotifyGameInitialized() {
        ValidateInitialization();
        Log("Game initialization", LogTag.Initialization);
        onGameInitialized.Invoke();
    }

    public void NotifyGameStateChanged() {
        ValidateInitialization();
        onGameStateChanged.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        ValidateInitialization();
        if (player == null) throw new System.ArgumentNullException(nameof(player));

        onPlayerDamaged.Invoke(player, damage);
        Log($"{damage} damage to {(player.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Players | LogTag.Combat);

        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreatureDamaged.Invoke(creature, damage);
        Log($"{damage} damage to {creature.Name}", LogTag.Creatures | LogTag.Combat);

        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreatureDied.Invoke(creature);
        Log($"Creature died: {creature.Name}", LogTag.Creatures);

        NotifyGameStateChanged();
    }

    public void NotifyGameOver(IPlayer winner) {
        ValidateInitialization();
        if (winner == null) throw new System.ArgumentNullException(nameof(winner));

        Log($"Game over: {(winner.IsPlayer1() ? "Player 1" : "Player 2")} wins", LogTag.Players);
        onGameOver.Invoke(winner);
    }

    public void NotifyCreaturePreSummon(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreaturePreSummon.Invoke(creature);
        Log($"Creature pre-summon: {creature.Name}", LogTag.Creatures);
    }

    public void NotifyCreatureSummoned(ICreature creature, IPlayer owner) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));
        if (owner == null) throw new System.ArgumentNullException(nameof(owner));

        onCreatureSummoned.Invoke(creature, owner);
        Log($"Creature summoned: {creature.Name} by {(owner.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Creatures);
        NotifyGameStateChanged();
    }

    private void ValidateInitialization() {
        if (!IsInitialized) {
            throw new System.InvalidOperationException("GameMediator is not initialized");
        }
    }

    private void ClearAllListeners() {
        onPlayerDamaged.RemoveAllListeners();
        onCreatureDamaged.RemoveAllListeners();
        onCreatureDied.RemoveAllListeners();
        onGameOver.RemoveAllListeners();
        onGameStateChanged.RemoveAllListeners();
        onGameInitialized.RemoveAllListeners();
        onCreatureSummoned.RemoveAllListeners();
        onCreaturePreSummon.RemoveAllListeners();
        registeredPlayers.Clear();
    }
}