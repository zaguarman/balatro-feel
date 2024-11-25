using UnityEngine.Events;
using System;

/// <summary>
/// Defines all possible game events that can be subscribed to.
/// This class only holds event definitions and does not handle game logic.
/// </summary>
public class GameEvents {
    [Serializable] public class PlayerDamagedEvent : UnityEvent<IPlayer, int> { }
    [Serializable] public class CreatureDamagedEvent : UnityEvent<ICreature, int> { }
    [Serializable] public class CreatureDiedEvent : UnityEvent<ICreature> { }
    [Serializable] public class GameOverEvent : UnityEvent<IPlayer> { }
    [Serializable] public class GameStateChangedEvent : UnityEvent { }
    [Serializable] public class GameInitializedEvent : UnityEvent { }

    // Event instances - read only properties
    public GameStateChangedEvent OnGameStateChanged { get; } = new GameStateChangedEvent();
    public PlayerDamagedEvent OnPlayerDamaged { get; } = new PlayerDamagedEvent();
    public CreatureDamagedEvent OnCreatureDamaged { get; } = new CreatureDamagedEvent();
    public CreatureDiedEvent OnCreatureDied { get; } = new CreatureDiedEvent();
    public GameOverEvent OnGameOver { get; } = new GameOverEvent();
    public GameInitializedEvent OnGameInitialized { get; } = new GameInitializedEvent();
}