using System;
using UnityEngine;

public class GameEventMediator : MonoBehaviour {
    private static GameEventMediator instance;
    public static GameEventMediator Instance {
        get {
            if (instance == null) {
                var go = new GameObject("GameEventMediator");
                instance = go.AddComponent<GameEventMediator>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public event Action<Creature, IPlayer> OnCreatureSummoned;
    public event Action<Creature, IPlayer> OnCreatureDied;

    public void NotifyCreatureSummoned(Creature creature, IPlayer owner) {
        Debug.Log($"Mediator: Creature summoned - {creature.Name}");
        OnCreatureSummoned?.Invoke(creature, owner);
    }

    public void NotifyCreatureDied(Creature creature, IPlayer owner) {
        Debug.Log($"Mediator: Creature died - {creature.Name}");
        OnCreatureDied?.Invoke(creature, owner);
    }
}