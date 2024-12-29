using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using static DebugLogger;
using static Enums;

public class ActionsQueue {
    private readonly List<IGameAction> actionsList = new List<IGameAction>();
    private readonly HashSet<(string, EffectTrigger)> processedEffects = new HashSet<(string, EffectTrigger)>();
    private readonly Dictionary<string, IGameAction> activeCreatureActions = new Dictionary<string, IGameAction>();
    private int currentIterationDepth = 0;
    private readonly int maxIterationDepth = 3;
    private readonly GameMediator gameMediator;
    private readonly BattlefieldCombatHandler combatHandler;

    public UnityEvent OnActionsQueued { get; } = new UnityEvent();
    public UnityEvent OnActionsResolved { get; } = new UnityEvent();

    public ActionsQueue(GameMediator gameMediator, BattlefieldCombatHandler combatHandler) {
        this.gameMediator = gameMediator;
        this.combatHandler = combatHandler;
    }

    private int GetActionPriority(IGameAction action) {
        return action switch {
            SummonCreatureAction => -1,
            MoveCreatureAction or SwapCreaturesAction => 0,
            PlayCardAction => 1,
            DirectDamageAction => 2,
            MarkCombatTargetAction => 3,
            DamageCreatureAction or DamagePlayerAction => 4,
            _ => 5
        };
    }

    private string GetActiveCreatureId(IGameAction action) {
        return action switch {
            DamageCreatureAction damageAction => damageAction.GetAttacker()?.TargetId,
            SwapCreaturesAction swapAction => swapAction.GetCreature1()?.TargetId,
            MarkCombatTargetAction combatAction => combatAction.GetAttacker()?.TargetId,
            _ => null
        };
    }

    public void AddAction(IGameAction action) {
        if (currentIterationDepth >= maxIterationDepth) {
            LogWarning("Maximum iteration depth reached, skipping action", LogTag.Actions);
            return;
        }

        string activeCreatureId = GetActiveCreatureId(action);

        if (activeCreatureId != null) {
            if (activeCreatureActions.ContainsKey(activeCreatureId)) {
                Log($"Replacing existing action for creature {activeCreatureId}", LogTag.Actions);
                actionsList.Remove(activeCreatureActions[activeCreatureId]);
            }
            activeCreatureActions[activeCreatureId] = action;
        }

        InsertActionWithPriority(action);
        Log($"Added action to queue: {action.GetType().Name}", LogTag.Actions);

        // log number of actions in the queue
        Log($"Actions in queue: {actionsList.Count}", LogTag.Actions);

        OnActionsQueued.Invoke();
        gameMediator.NotifyGameStateChanged();
    }

    private void InsertActionWithPriority(IGameAction action) {
        int priority = GetActionPriority(action);
        int insertIndex = actionsList.Count;

        for (int i = 0; i < actionsList.Count; i++) {
            if (GetActionPriority(actionsList[i]) > priority) {
                insertIndex = i;
                break;
            }
        }

        actionsList.Insert(insertIndex, action);
        Log($"Inserted action {action.GetType().Name} at priority {priority}, position {insertIndex}", LogTag.Actions);
    }

    public bool IsEffectProcessed(string sourceId, EffectTrigger trigger) {
        bool isProcessed = processedEffects.Contains((sourceId, trigger));
        if (isProcessed) {
            Log($"Effect {trigger} for creature {sourceId} has already been processed", LogTag.Effects);
        }
        return isProcessed;
    }

    public void MarkEffectProcessed(string sourceId, EffectTrigger trigger) {
        processedEffects.Add((sourceId, trigger));
        Log($"Marked effect {trigger} for creature {sourceId} as processed", LogTag.Effects);
    }

    public void ResolveActions() {
        // Early return if there are no actions to process
        if (actionsList.Count == 0) {
            Log("No actions to resolve", LogTag.Actions);
            return;
        }

        currentIterationDepth++;
        Log($"Resolving actions. Queue size: {actionsList.Count}", LogTag.Actions);

        processedEffects.Clear();
        Log("Cleared processed effects for new resolution chain", LogTag.Effects);

        while (actionsList.Count > 0) {
            var action = actionsList[0];
            actionsList.RemoveAt(0);

            string activeCreatureId = GetActiveCreatureId(action);
            if (activeCreatureId != null) {
                activeCreatureActions.Remove(activeCreatureId);
            }

            Log($"Executing action: {action.GetType().Name}", LogTag.Actions);
            action.Execute();
        }

        currentIterationDepth--;
        activeCreatureActions.Clear();
        combatHandler?.ResetAttackingCreatures();

        OnActionsResolved.Invoke();
        gameMediator.NotifyGameStateChanged();
        Log("Action resolution complete", LogTag.Actions);
    }

    public bool HasActiveAction(string creatureId) {
        return activeCreatureActions.ContainsKey(creatureId);
    }

    public IGameAction GetActiveAction(string creatureId) {
        activeCreatureActions.TryGetValue(creatureId, out var action);
        return action;
    }

    public int GetPendingActionsCount() => actionsList.Count;

    public IReadOnlyCollection<IGameAction> GetPendingActions() => actionsList.AsReadOnly();

    public void Cleanup() {
        actionsList.Clear();
        activeCreatureActions.Clear();
        processedEffects.Clear();
        OnActionsQueued.RemoveAllListeners();
        OnActionsResolved.RemoveAllListeners();
        Log("Actions queue cleaned up", LogTag.Actions);
    }
}