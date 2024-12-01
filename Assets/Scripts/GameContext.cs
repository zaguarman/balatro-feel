using System.Collections.Generic;
using UnityEngine;

public class GameContext {
    private Queue<IGameAction> actionQueue = new Queue<IGameAction>();
    private int maxIterationDepth = 3;
    private int currentIterationDepth = 0;
    public TargetingSystem TargetingSystem { get; private set; }
    private Dictionary<ICard, IPlayer> cardOwners = new Dictionary<ICard, IPlayer>();

    public GameContext() {
        TargetingSystem = new TargetingSystem();
    }

    public void RegisterCardOwner(ICard card, IPlayer owner) {
        cardOwners[card] = owner;
    }

    public IPlayer GetOwner(ICard card) {
        return cardOwners.TryGetValue(card, out var owner) ? owner : null;
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