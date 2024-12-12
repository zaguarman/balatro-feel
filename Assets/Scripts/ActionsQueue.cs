using System.Collections.Generic;
using static DebugLogger;

public class ActionsQueue {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    public void AddAction(IGameAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            actionQueue.Enqueue(action);
            Log($"Added action to queue: {action.GetType()}", LogTag.Actions);
        }
    }

    public void ResolveActions() {
        currentIterationDepth++;
        Log($"Resolving actions. Queue size: {actionQueue.Count}", LogTag.Actions);

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