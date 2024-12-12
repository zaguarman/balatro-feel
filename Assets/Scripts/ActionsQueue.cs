using System.Collections.Generic;
using UnityEngine;

public class ActionsQueue {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    public void AddAction(IGameAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            actionQueue.Enqueue(action);
            DebugLogger.Log($"Added action to queue: {action.GetType()}", LogTag.Actions);
        }
    }

    public void ResolveActions() {
        currentIterationDepth++;
        DebugLogger.Log($"Resolving actions. Queue size: {actionQueue.Count}", LogTag.Actions);

        while (actionQueue.Count > 0) {
            var action = actionQueue.Dequeue();
            action.Execute();
        }

        currentIterationDepth--;
    }

    public int GetPendingActionsCount() {
        return actionQueue.Count;
    }

    public IReadOnlyCollection<IGameAction> GetPendingActions() {
        return actionQueue.ToArray();
    }
}