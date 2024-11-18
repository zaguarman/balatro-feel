using System;
using System.Collections.Generic;
using UnityEngine;

public class GameContext {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    public void AddAction(IGameAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            actionQueue.Enqueue(action);
        }
    }

    public void ResolveActions() {
        currentIterationDepth++;
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