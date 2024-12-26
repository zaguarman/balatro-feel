using UnityEngine;
using System.Collections.Generic;
using static DebugLogger;
using System.Linq;

public class BattlefieldArrowManager {
    private readonly Transform parentTransform;
    private readonly GameManager gameManager;
    private readonly GameReferences gameReferences;
    private ArrowIndicator dragArrowIndicator;
    private Dictionary<string, ArrowIndicator> activeArrows = new Dictionary<string, ArrowIndicator>();

    public BattlefieldArrowManager(Transform parent, GameManager gameManager) {
        this.parentTransform = parent;
        this.gameManager = gameManager;
        this.gameReferences = GameReferences.Instance;
        SetupDragArrow();
    }

    private void SetupDragArrow() {
        dragArrowIndicator = ArrowIndicator.Create(parentTransform);
        dragArrowIndicator.Hide();
    }

    public void ShowDragArrow(Vector3 startPos) {
        startPos.z = 0;
        dragArrowIndicator.Show(startPos, startPos);
    }

    public void UpdateDragArrow(Vector3 worldPos) {
        if (dragArrowIndicator != null && dragArrowIndicator.IsVisible()) {
            worldPos.z = 0;
            dragArrowIndicator.UpdateEndPosition(worldPos);
        }
    }

    public void HideDragArrow() {
        if (dragArrowIndicator != null) {
            dragArrowIndicator.Hide();
        }
    }

    public void UpdateArrowsFromActionsQueue() {
        Log("Starting update of arrows from actions queue", LogTag.Actions);

        // Clear existing arrows before recreating
        ClearExistingArrows();

        if (gameManager.ActionsQueue == null) {
            LogWarning("ActionsQueue is null!", LogTag.Actions);
            return;
        }

        var pendingActions = gameManager.ActionsQueue.GetPendingActions();
        Log($"Number of pending actions: {pendingActions.Count}", LogTag.Actions);

        // Process all pending actions instead of just the last one
        ProcessQueuedActions(pendingActions);
    }

    private void ClearExistingArrows() {
        foreach (var arrow in activeArrows.Values) {
            if (arrow != null) {
                Object.Destroy(arrow.gameObject);
            }
        }
        activeArrows.Clear();
    }

    private void ProcessQueuedActions(IReadOnlyCollection<IGameAction> actions) {
        // Remove arrows for actions no longer in queue
        var currentArrowKeys = activeArrows.Keys.ToList();
        foreach (var key in currentArrowKeys) {
            if (!actions.Any(a => GetActionKey(a) == key)) {
                if (activeArrows[key] != null) {
                    Object.Destroy(activeArrows[key].gameObject);
                }
                activeArrows.Remove(key);
            }
        }

        // Process each action
        foreach (var action in actions) {
            ProcessAction(action);
        }
    }

    private string GetActionKey(IGameAction action) {
        return action switch {
            MarkCombatTargetAction markCombatAction => $"combat_{markCombatAction.GetAttacker()?.TargetId}",
            DamageCreatureAction damageAction => $"damage_{damageAction.GetAttacker()?.TargetId}",
            DamagePlayerAction playerDamageAction => $"playerDamage_{FindSourceCreatureForPlayerDamage(playerDamageAction)?.TargetId}",
            MoveCreatureAction moveAction => $"move_{moveAction.GetCreature()?.TargetId}",
            _ => null
        };
    }

    private void ProcessAction(IGameAction action) {
        string actionKey = GetActionKey(action);
        if (actionKey == null) return;

        // Only create new arrow if one doesn't exist for this action
        if (!activeArrows.ContainsKey(actionKey)) {
            switch (action) {
                case MarkCombatTargetAction markCombatAction:
                    CreateArrowForMarkCombatAction(markCombatAction, actionKey);
                    break;
                case DamageCreatureAction damageAction:
                    CreateArrowForDamageAction(damageAction, actionKey);
                    break;
                case DamagePlayerAction playerDamageAction:
                    CreateArrowForPlayerDamageAction(playerDamageAction, actionKey);
                    break;
                case MoveCreatureAction moveAction:
                    CreateArrowForMoveAction(moveAction, actionKey);
                    break;
            }
        }
    }

    private void CreateArrowForMoveAction(MoveCreatureAction moveAction, string actionKey) {
        var creature = moveAction.GetCreature();
        if (creature == null) return;

        var arrow = ArrowIndicator.Create(parentTransform);
        Vector3 startPos = GetCreaturePosition(creature);
        Vector3 endPos = GetSlotPosition(moveAction.GetToSlot());

        startPos.z = 0;
        endPos.z = 0;

        arrow.Show(startPos, endPos);
        arrow.SetColor(Color.green); // Use green for move actions
        activeArrows[actionKey] = arrow;

        Log($"Created move arrow for {creature.Name} to slot {moveAction.GetToSlot()}", LogTag.Actions | LogTag.UI);
    }

    private Vector3 GetSlotPosition(int slotIndex) {
        var player1Battlefield = gameReferences.GetPlayer1BattlefieldUI();
        var player2Battlefield = gameReferences.GetPlayer2BattlefieldUI();

        // Try to find the slot in either battlefield
        Transform slotTransform = null;
        if (player1Battlefield != null) {
            var slots = player1Battlefield.GetComponentsInChildren<BattlefieldSlot>();
            slotTransform = slots.FirstOrDefault(s => s.Index == slotIndex)?.transform;
        }

        if (slotTransform == null && player2Battlefield != null) {
            var slots = player2Battlefield.GetComponentsInChildren<BattlefieldSlot>();
            slotTransform = slots.FirstOrDefault(s => s.Index == slotIndex)?.transform;
        }

        return slotTransform != null ? slotTransform.position : Vector3.zero;
    }

    private void CreateArrowForMarkCombatAction(MarkCombatTargetAction action, string actionKey) {
        var attacker = action.GetAttacker();
        var targetSlot = action.GetTargetSlot();

        if (attacker == null || targetSlot == null) return;

        var arrow = ArrowIndicator.Create(parentTransform);
        Vector3 startPos = GetCreaturePosition(attacker);
        Vector3 endPos = targetSlot.transform.position;

        startPos.z = 0;
        endPos.z = 0;

        arrow.Show(startPos, endPos);
        activeArrows[actionKey] = arrow;

        Log($"Created combat targeting arrow from {attacker.Name} to slot {targetSlot.Index}", LogTag.Actions | LogTag.UI);
    }

    private void CreateArrowForDamageAction(DamageCreatureAction damageAction, string actionKey) {
        var attacker = damageAction.GetAttacker();
        var target = damageAction.GetTarget();

        if (attacker == null || target == null) return;

        var arrow = ArrowIndicator.Create(parentTransform);
        Vector3 startPos = GetCreaturePosition(attacker);
        Vector3 endPos = GetCreaturePosition(target);

        startPos.z = 0;
        endPos.z = 0;

        arrow.Show(startPos, endPos);
        activeArrows[actionKey] = arrow;

        Log($"Created arrow from {attacker.Name} to {target.Name}", LogTag.Actions | LogTag.UI);
    }

    private void CreateArrowForPlayerDamageAction(DamagePlayerAction action, string actionKey) {
        if (action == null) return;

        var sourceCreature = FindSourceCreatureForPlayerDamage(action);
        if (sourceCreature == null) return;

        var targetPlayer = action.GetTargetPlayer();
        if (targetPlayer == null) return;

        var arrow = ArrowIndicator.Create(parentTransform);
        Vector3 startPos = GetCreaturePosition(sourceCreature);
        Vector3 endPos = GetPlayerTargetPosition(targetPlayer);

        startPos.z = 0;
        endPos.z = 0;

        arrow.Show(startPos, endPos);
        activeArrows[actionKey] = arrow;

        Log($"Created arrow from {sourceCreature.Name} to Player {(targetPlayer.IsPlayer1() ? "1" : "2")}",
            LogTag.Actions | LogTag.UI);
    }

    private ICreature FindSourceCreatureForPlayerDamage(DamagePlayerAction action) {
        // Look through the battlefield to find the attacking creature
        var attackingCreature = gameManager.Player1.Battlefield
            .FirstOrDefault(c => gameManager.CombatHandler.HasCreatureAttacked(c.TargetId));

        if (attackingCreature == null) {
            attackingCreature = gameManager.Player2.Battlefield
                .FirstOrDefault(c => gameManager.CombatHandler.HasCreatureAttacked(c.TargetId));
        }

        return attackingCreature;
    }

    private Vector3 GetCreaturePosition(ICreature creature) {
        if (creature == null) return Vector3.zero;

        // Try to find the card in Player 1's battlefield
        var player1Battlefield = gameReferences.GetPlayer1BattlefieldUI();
        var cardController = player1Battlefield?.GetCardControllerByCreatureId(creature.TargetId);

        // If not found in Player 1's battlefield, try Player 2's
        if (cardController == null) {
            var player2Battlefield = gameReferences.GetPlayer2BattlefieldUI();
            cardController = player2Battlefield?.GetCardControllerByCreatureId(creature.TargetId);
        }

        if (cardController != null) {
            var position = cardController.transform.position;
            Log($"Found position for creature {creature.Name} with ID {creature.TargetId}: {position}", LogTag.Actions);
            return position;
        }

        LogWarning($"Could not find CardController for creature {creature.Name} with ID {creature.TargetId}", LogTag.Actions);
        return Vector3.zero;
    }

    private Vector3 GetPlayerTargetPosition(IPlayer player) {
        var playerUI = player.IsPlayer1() ?
            gameReferences.GetPlayer1UI() :
            gameReferences.GetPlayer2UI();

        if (playerUI != null) {
            return playerUI.transform.position;
        }

        LogWarning($"Could not find UI for Player {(player.IsPlayer1() ? "1" : "2")}", LogTag.Actions);
        return Vector3.zero;
    }

    public void Cleanup() {
        ClearExistingArrows();
        if (dragArrowIndicator != null) {
            Object.Destroy(dragArrowIndicator.gameObject);
        }
    }
}