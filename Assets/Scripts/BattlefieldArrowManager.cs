using UnityEngine;
using System.Collections.Generic;
using static DebugLogger;

public class BattlefieldArrowManager {
    private readonly Transform parentTransform;
    private readonly GameManager gameManager;
    private readonly GameReferences gameReferences;
    private ArrowIndicator dragArrowIndicator;
    private Dictionary<string, ArrowIndicator> activeArrows = new Dictionary<string, ArrowIndicator>();

    // Deep green for player 1's arrows (#20853E)
    private static readonly Color player1ArrowColor = new Color(0.125f, 0.522f, 0.243f, 0.9f);
    // Deep red for player 2's arrows (#820807)
    private static readonly Color player2ArrowColor = new Color(0.510f, 0.031f, 0.027f, 0.9f);

    public BattlefieldArrowManager(Transform parent, GameManager gameManager) {
        this.parentTransform = parent;
        this.gameManager = gameManager;
        this.gameReferences = GameReferences.Instance;
        SetupDragArrow();
    }

    private void SetupDragArrow() {
        dragArrowIndicator = ArrowIndicator.Create(parentTransform);
        dragArrowIndicator.SetColor(player1ArrowColor); // Default to player 1 color
        dragArrowIndicator.Hide();
    }

    public void ShowDragArrow(Vector3 startPos, bool isPlayer1 = true) {
        startPos.z = 0;
        dragArrowIndicator.SetColor(isPlayer1 ? player1ArrowColor : player2ArrowColor);
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

        ClearExistingArrows();

        if (gameManager.ActionsQueue == null) {
            LogWarning("ActionsQueue is null!", LogTag.Actions);
            return;
        }

        foreach (var action in gameManager.ActionsQueue.GetPendingActions()) {
            ProcessAction(action);
        }
    }

    private void ClearExistingArrows() {
        foreach (var arrow in activeArrows.Values) {
            if (arrow != null) {
                Object.Destroy(arrow.gameObject);
            }
        }
        activeArrows.Clear();
    }

    private void ProcessAction(IGameAction action) {
        if (action is DamageCreatureAction damageAction) {
            CreateArrowForDamageAction(damageAction);
        }
    }

    private void CreateArrowForDamageAction(DamageCreatureAction damageAction) {
        var attacker = damageAction.GetAttacker();
        var target = damageAction.GetTarget();

        if (attacker == null || target == null) return;

        var arrow = ArrowIndicator.Create(parentTransform);
        Vector3 startPos = GetCreaturePosition(attacker);
        Vector3 endPos = GetCreaturePosition(target);

        startPos.z = 0;
        endPos.z = 0;

        // Determine if the attacker belongs to player 1
        bool isPlayer1Attacker = gameManager.Player1.Battlefield.Contains(attacker);
        arrow.SetColor(isPlayer1Attacker ? player1ArrowColor : player2ArrowColor);
        arrow.Show(startPos, endPos);
        activeArrows[attacker.TargetId] = arrow;
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

    public void Cleanup() {
        ClearExistingArrows();
        if (dragArrowIndicator != null) {
            Object.Destroy(dragArrowIndicator.gameObject);
        }
    }
}