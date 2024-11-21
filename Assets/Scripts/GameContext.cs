using System.Collections.Generic;
using UnityEngine;

public class GameContext {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;
    public TargetingSystem TargetingSystem { get; private set; }

    public GameContext() {
        TargetingSystem = new TargetingSystem();
    }

    public void AddAction(IGameAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            actionQueue.Enqueue(action);
            Debug.Log($"Added action to queue: {action.GetType()}");
        }
    }

    public void ResolveActions() {
        currentIterationDepth++;
        Debug.Log($"Resolving actions. Queue size: {actionQueue.Count}");
        while (actionQueue.Count > 0) {
            var action = actionQueue.Dequeue();
            action.Execute(this);
        }
        currentIterationDepth--;
    }

    public int GetPendingActionsCount() {
        return actionQueue.Count;
    }
}