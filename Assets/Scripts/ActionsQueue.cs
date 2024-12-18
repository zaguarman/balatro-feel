using static DebugLogger;
using System.Collections.Generic;
using UnityEngine.Events;

public class ActionsQueue {
    private List<IGameAction> actionsList = new List<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;
    private Dictionary<string, DamageCreatureAction> pendingDamageActions = new Dictionary<string, DamageCreatureAction>();
    private readonly GameMediator gameMediator;
    private readonly BattlefieldCombatHandler combatHandler;

    public readonly UnityEvent OnActionsQueued = new UnityEvent();
    public readonly UnityEvent OnActionsResolved = new UnityEvent();

    public ActionsQueue(GameMediator gameMediator, BattlefieldCombatHandler combatHandler) {
        this.gameMediator = gameMediator;
        this.combatHandler = combatHandler;
    }

    private int GetActionPriority(IGameAction action) {
        return action switch {
            DirectDamageAction => 0,  // Highest priority
            SwapCreaturesAction => 1, // Medium priority
            DamageCreatureAction => 2, // Lowest priority
            _ => 3 // All other actions
        };
    }

    public void AddAction(IGameAction action) {
        if (currentIterationDepth >= maxIterationDepth) {
            LogWarning("Maximum iteration depth reached, skipping action", LogTag.Actions);
            return;
        }

        // Handle DamageCreatureAction specially for pending actions system
        if (action is DamageCreatureAction damageAction) {
            HandleDamageAction(damageAction);
        } else {
            InsertActionWithPriority(action);
            Log($"Added action to queue: {action.GetType()}", LogTag.Actions);
        }

        OnActionsQueued.Invoke();
        gameMediator.NotifyGameStateChanged();
    }

    private void InsertActionWithPriority(IGameAction action) {
        int priority = GetActionPriority(action);
        int insertIndex = actionsList.Count;

        // Find the correct position to insert the action based on priority
        for (int i = 0; i < actionsList.Count; i++) {
            if (GetActionPriority(actionsList[i]) > priority) {
                insertIndex = i;
                break;
            }
        }

        actionsList.Insert(insertIndex, action);
    }

    private void HandleDamageAction(DamageCreatureAction damageAction) {
        var attacker = damageAction.GetAttacker();
        if (attacker == null) {
            InsertActionWithPriority(damageAction);
            return;
        }

        string attackerId = attacker.TargetId;
        if (pendingDamageActions.ContainsKey(attackerId)) {
            Log($"Replacing existing damage action for {attacker.Name}", LogTag.Actions);
            actionsList.RemoveAll(action => action == pendingDamageActions[attackerId]);
        }

        pendingDamageActions[attackerId] = damageAction;
        InsertActionWithPriority(damageAction);
        Log($"Added/Updated damage action for {attacker.Name}", LogTag.Actions);
    }

    public void ResolveActions() {
        currentIterationDepth++;
        Log($"Resolving actions. Queue size: {actionsList.Count}", LogTag.Actions);

        while (actionsList.Count > 0) {
            var action = actionsList[0];
            actionsList.RemoveAt(0);

            if (action is DamageCreatureAction damageAction) {
                var attacker = damageAction.GetAttacker();
                if (attacker != null) {
                    pendingDamageActions.Remove(attacker.TargetId);
                }
            }
            action.Execute();
        }

        currentIterationDepth--;
        pendingDamageActions.Clear();

        // Reset attacking creatures after actions are resolved
        if (combatHandler != null) {
            combatHandler.ResetAttackingCreatures();
        }

        OnActionsResolved.Invoke();
        gameMediator.NotifyGameStateChanged();
    }

    public int GetPendingActionsCount() => actionsList.Count;

    public IReadOnlyCollection<IGameAction> GetPendingActions() => actionsList.AsReadOnly();

    public void Cleanup() {
        actionsList.Clear();
        pendingDamageActions.Clear();
        OnActionsQueued.RemoveAllListeners();
        OnActionsResolved.RemoveAllListeners();
    }
}