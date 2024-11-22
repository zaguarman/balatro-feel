using UnityEngine;
using UnityEngine.Events;
using System;

public class GameEvents : Singleton<GameEvents> {
    [Serializable] public class PlayerDamagedEvent : UnityEvent<IPlayer, int> { }
    [Serializable] public class CreatureDamagedEvent : UnityEvent<ICreature, int> { }
    [Serializable] public class CreatureDiedEvent : UnityEvent<ICreature> { }
    [Serializable] public class GameOverEvent : UnityEvent<IPlayer> { }
    [Serializable] public class GameStateChangedEvent : UnityEvent { }

    public GameStateChangedEvent OnGameStateChanged { get; } = new GameStateChangedEvent();
    public PlayerDamagedEvent OnPlayerDamaged { get; } = new PlayerDamagedEvent();
    public CreatureDamagedEvent OnCreatureDamaged { get; } = new CreatureDamagedEvent();
    public CreatureDiedEvent OnCreatureDied { get; } = new CreatureDiedEvent();
    public GameOverEvent OnGameOver { get; } = new GameOverEvent();

    public void NotifyGameStateChanged() {
        Debug.Log("Game state changed");
        OnGameStateChanged.Invoke();
    }

    public void NotifyPlayerDamaged(IPlayer player, int damage) {
        Debug.Log($"Player {player.TargetId} damaged for {damage}");
        OnPlayerDamaged.Invoke(player, damage);
        if (player.Health <= 0) {
            NotifyGameOver(player.Opponent);
        }
    }

    public void NotifyCreatureDamaged(ICreature creature, int damage) {
        Debug.Log($"Creature {creature.Name} damaged for {damage}");
        OnCreatureDamaged.Invoke(creature, damage);
        if (creature.Health <= 0) {
            NotifyCreatureDied(creature);
        }
    }

    public void NotifyCreatureDied(ICreature creature) {
        Debug.Log($"Creature {creature.Name} died");
        OnCreatureDied.Invoke(creature);
    }

    public void NotifyGameOver(IPlayer winner) {
        Debug.Log($"Game over - Winner: {winner.TargetId}");
        OnGameOver.Invoke(winner);
    }
} 