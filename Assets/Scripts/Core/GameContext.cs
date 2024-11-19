using System.Collections.Generic;
using UnityEngine;

public interface IGameContext {
    IAction LastAction { get; }
    IPhaseStrategy CurrentPhase { get; }
    void AddAction(IAction action);
    void TransitionToPhase(IPhaseStrategy newPhase);
}

public class GameContext : IGameContext {
    private Queue<IAction> actionQueue = new Queue<IAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    public IAction LastAction { get; private set; }
    public IPhaseStrategy CurrentPhase { get; private set; }

    public void AddAction(IAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            actionQueue.Enqueue(action);
            Debug.Log($"Action queued: {action.GetType().Name}. Queue size: {actionQueue.Count}");
        }
    }

    public void TransitionToPhase(IPhaseStrategy newPhase) {
        if (CurrentPhase?.CanTransitionTo(newPhase) ?? true) {
            CurrentPhase?.ExitPhase(this);
            CurrentPhase = newPhase;
            CurrentPhase.EnterPhase(this);
        }
    }

    public void ResolveActions() {
        currentIterationDepth++;
        while (actionQueue.Count > 0) {
            var action = actionQueue.Dequeue();
            Debug.Log($"Executing action: {action.GetType().Name}");
            LastAction = action;
            action.Execute(this);
        }
        currentIterationDepth--;
        Debug.Log("Finished resolving all actions");
    }

    public int GetPendingActionsCount() {
        return actionQueue.Count;
    }
}
