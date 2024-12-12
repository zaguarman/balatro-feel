using static DebugLogger;
using System.Collections.Generic;
using System.Linq;

public class ActionsQueue {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    // Add event for action resolution
    public event System.Action OnActionsResolved;

    public void AddAction(IGameAction action) {
        if (currentIterationDepth < maxIterationDepth) {
            // If it's a damage action, check if we need to replace an existing one
            if (action is DamageCreatureAction damageAction) {
                RemoveExistingDamageAction(damageAction.GetAttacker());
            }

            actionQueue.Enqueue(action);
            Log($"Added action to queue: {action.GetType()}", LogTag.Actions);
        }
    }

    private void RemoveExistingDamageAction(ICreature attacker) {
        if (attacker == null) return;

        var currentActions = actionQueue.ToList();
        bool found = false;

        // Clear the queue
        actionQueue.Clear();

        // Re-add actions, skipping the old damage action from the same attacker
        foreach (var existingAction in currentActions) {
            if (existingAction is DamageCreatureAction existingDamage &&
                existingDamage.GetAttacker()?.TargetId == attacker.TargetId) {
                found = true;
                Log($"Removing existing damage action for {attacker.Name}", LogTag.Actions);
                continue;
            }
            actionQueue.Enqueue(existingAction);
        }

        if (found) {
            Log($"Replaced damage action for {attacker.Name}", LogTag.Actions);
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

        // Notify listeners that actions have been resolved
        OnActionsResolved?.Invoke();
    }

    public int GetPendingActionsCount() {
        return actionQueue.Count;
    }

    public IReadOnlyCollection<IGameAction> GetPendingActions() {
        return actionQueue.ToArray();
    }
}