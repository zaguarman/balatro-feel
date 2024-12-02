using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

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
            //Debug.Log($"Player registered with GameMediator: {(player.IsPlayer1() ? "Player 1" : "Player 2")}");
        }
    }

    public void UnregisterPlayer(IPlayer player) {
        if (player == null) return;
        if (registeredPlayers.Remove(player)) {
            //Debug.Log($"Player unregistered from GameMediator: {(player.IsPlayer1() ? "Player 1" : "Player 2")}");
        }
    }

    public void NotifyGameInitialized() {
        ValidateInitialization();
        //Debug.Log("Game initialization notification sent");
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
        Debug.Log($"Player damaged notification: {damage} damage to {(player.IsPlayer1() ? "Player 1" : "Player 2")}");

        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreatureDamaged.Invoke(creature, damage);
        Debug.Log($"Creature damaged notification: {damage} damage to {creature.Name}");

        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreatureDied.Invoke(creature);
        Debug.Log($"Creature died notification: {creature.Name}");

        NotifyGameStateChanged();
    }

    public void NotifyGameOver(IPlayer winner) {
        ValidateInitialization();
        if (winner == null) throw new System.ArgumentNullException(nameof(winner));

        Debug.Log($"Game over notification: {(winner.IsPlayer1() ? "Player 1" : "Player 2")} wins");
        onGameOver.Invoke(winner);
    }

    public void NotifyCreaturePreSummon(ICreature creature) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));

        onCreaturePreSummon.Invoke(creature);
        //Debug.Log($"Creature pre-summon notification: {creature.Name}");
    }

    public void NotifyCreatureSummoned(ICreature creature, IPlayer owner) {
        ValidateInitialization();
        if (creature == null) throw new System.ArgumentNullException(nameof(creature));
        if (owner == null) throw new System.ArgumentNullException(nameof(owner));

        onCreatureSummoned.Invoke(creature, owner);
        Debug.Log($"Creature summoned notification: {creature.Name} by {(owner.IsPlayer1() ? "Player 1" : "Player 2")}");
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