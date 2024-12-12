using System.Collections.Generic;
using System.Linq;
using static DebugLogger;

public class ActionsQueue {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private HashSet<string> attackingCreatureIds = new HashSet<string>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;

    public event System.Action OnActionsResolved;

    public void AddAction(IGameAction action) {
        if (currentIterationDepth >= maxIterationDepth) return;

        if (action is DamageCreatureAction damageAction) {
            HandleDamageAction(damageAction);
        } else {
            actionQueue.Enqueue(action);
        }

        Log($"Added action to queue: {action.GetType()}", LogTag.Actions);
    }

    private void HandleDamageAction(DamageCreatureAction newAction) {
        var attacker = newAction.GetAttacker();
        if (attacker == null) {
            actionQueue.Enqueue(newAction);
            return;
        }

        // Remove any existing attacks from this creature
        RemoveExistingAttacks(attacker.TargetId);

        // Add the new attack and mark the creature as having attacked
        actionQueue.Enqueue(newAction);
        attackingCreatureIds.Add(attacker.TargetId);

        Log($"Updated attack for creature {attacker.Name}", LogTag.Actions | LogTag.Combat);
    }

    private void RemoveExistingAttacks(string attackerId) {
        var remainingActions = actionQueue
            .Where(action => !(action is DamageCreatureAction damageAction &&
                             damageAction.GetAttacker()?.TargetId == attackerId))
            .ToList();

        actionQueue.Clear();
        foreach (var action in remainingActions) {
            actionQueue.Enqueue(action);
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
        ResetAttackingCreatures();
        OnActionsResolved?.Invoke();
    }

    public void ResetAttackingCreatures() {
        attackingCreatureIds.Clear();
        Log("Reset attacking creatures", LogTag.Creatures | LogTag.Combat);
    }

    public bool HasCreatureAttacked(string creatureId) {
        return attackingCreatureIds.Contains(creatureId);
    }

    public int GetPendingActionsCount() {
        return actionQueue.Count;
    }

    public IReadOnlyCollection<IGameAction> GetPendingActions() {
        return actionQueue.ToArray();
    }
}
