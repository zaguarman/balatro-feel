using static DebugLogger;
using System.Collections.Generic;
using System.Linq;

public class BattlefieldCombatHandler {
    private readonly GameManager gameManager;
    private HashSet<string> attackingCreatureIds = new HashSet<string>();
    private readonly Dictionary<string, BattlefieldSlot> targetedSlots = new Dictionary<string, BattlefieldSlot>();
    private readonly Dictionary<CardController, ICreature> creatureCache = new Dictionary<CardController, ICreature>();

    public BattlefieldCombatHandler(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void HandleCreatureCombat(CardController attackingCard, BattlefieldSlot targetSlot) {
        var attackerCreature = GetCachedCreature(attackingCard);

        if (attackerCreature == null) {
            LogWarning("Attacker creature is null", LogTag.Creatures | LogTag.Actions);
            return;
        }

        if (HasCreatureAttacked(attackerCreature.TargetId)) {
            LogWarning($"Creature {attackerCreature.Name} has already attacked this turn", LogTag.Creatures | LogTag.Combat);
            return;
        }

        if (targetSlot == null) {
            LogWarning("Target slot is null", LogTag.Creatures | LogTag.Combat);
            return;
        }

        RegisterAttack(attackerCreature.TargetId, targetSlot);
        QueueCombatAction(attackerCreature, targetSlot);
    }

    private ICreature GetCachedCreature(CardController cardController) {
        if (cardController == null) return null;

        if (creatureCache.TryGetValue(cardController, out var cachedCreature)) {
            return cachedCreature;
        }

        string targetId = cardController.GetLinkedCreatureId();
        if (string.IsNullOrEmpty(targetId)) return null;

        var creature = gameManager.Player1.Battlefield.FirstOrDefault(c => c.TargetId == targetId) ??
                      gameManager.Player2.Battlefield.FirstOrDefault(c => c.TargetId == targetId);

        if (creature != null) {
            creatureCache[cardController] = creature;
        }

        return creature;
    }

    private void RegisterAttack(string attackerId, BattlefieldSlot targetSlot) {
        attackingCreatureIds.Add(attackerId);
        targetedSlots[attackerId] = targetSlot;
    }

    private void QueueCombatAction(ICreature attackerCreature, BattlefieldSlot targetSlot) {
        gameManager.ActionsQueue.AddAction(new MarkCombatTargetAction(attackerCreature, targetSlot));
        Log($"{attackerCreature.Name} targets slot {targetSlot.Index}", LogTag.Creatures | LogTag.Combat);
    }

    public void ResetAttackingCreatures() {
        attackingCreatureIds.Clear();
        targetedSlots.Clear();
        creatureCache.Clear();
        Log("Reset attacking creatures tracking", LogTag.Creatures | LogTag.Combat);
    }

    public bool HasCreatureAttacked(string creatureId) {
        return attackingCreatureIds.Contains(creatureId);
    }

    public BattlefieldSlot GetTargetedSlot(string attackerId) {
        return targetedSlots.TryGetValue(attackerId, out var slot) ? slot : null;
    }
}