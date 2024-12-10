using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlefieldUI : CardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();
    protected GameManager gameManager;
    private ArrowIndicator dragArrowIndicator;
    private CardController attackingCard;
    private Dictionary<string, ArrowIndicator> activeArrows = new Dictionary<string, ArrowIndicator>();

    private void Start() {
        gameManager = GameManager.Instance;
        SetupDragArrow();
    }

    private void SetupDragArrow() {
        dragArrowIndicator = ArrowIndicator.Create(transform);
        dragArrowIndicator.Hide();
    }

    protected override void HandleCardDropped(CardController card) {
        if (card == null || !CanAcceptCard(card)) return;

        if (gameManager == null) {
            Debug.LogError("GameManager reference is null in BattlefieldUI");
            return;
        }

        // Check if this is a new creature being played from hand
        if (card.transform.parent.GetComponent<HandUI>() != null) {
            HandleNewCreature(card);
        }
        // Handle combat - creature attacking another creature
        else if (card.transform.parent.GetComponent<BattlefieldUI>() != null) {
            HandleCreatureCombat(card);
        }
    }

    private void HandleNewCreature(CardController card) {
        if (card.GetCardData() is CreatureData creatureData) {
            gameManager.PlayCard(creatureData, card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2);
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private void HandleCreatureCombat(CardController attackingCard) {
        Debug.Log($"[Combat] Starting combat with attacker: {attackingCard.GetCardData().cardName}");

        // Convert mouse position to world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Ensure we're on the same Z plane as the cards

        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        Debug.Log($"[Combat] Found {raycastResults.Count} potential targets under mouse position");

        foreach (var result in raycastResults) {
            var targetCardController = result.gameObject.GetComponent<CardController>();
            if (targetCardController != null && targetCardController != attackingCard) {
                Debug.Log($"[Combat] Found target card: {targetCardController.GetCardData().cardName}");

                // Ensure the target is an enemy creature
                bool isEnemyCreature = targetCardController.IsPlayer1Card() != attackingCard.IsPlayer1Card();
                Debug.Log($"[Combat] Is enemy creature? {isEnemyCreature}");

                if (isEnemyCreature) {
                    // Get the actual creatures from the battlefield
                    var attackerCreature = FindCreatureByTargetId(attackingCard);
                    var targetCreature = FindCreatureByTargetId(targetCardController);

                    Debug.Log($"[Combat] Found attacker creature: {(attackerCreature != null ? attackerCreature.Name : "null")}");
                    Debug.Log($"[Combat] Found target creature: {(targetCreature != null ? targetCreature.Name : "null")}");

                    if (attackerCreature != null && targetCreature != null) {
                        Debug.Log($"[Combat] Creating DamageCreatureAction - Attacker: {attackerCreature.Name}, Attack: {attackerCreature.Attack}, Target: {targetCreature.Name}, Current Health: {targetCreature.Health}");

                        // Create damage action including the attacker reference
                        var damageAction = new DamageCreatureAction(targetCreature, attackerCreature.Attack, attackerCreature);
                        gameManager.ActionsQueue.AddAction(damageAction);

                        Debug.Log($"[Combat] Added damage action to queue. Queue size: {gameManager.ActionsQueue.GetPendingActionsCount()}");

                        // Force an immediate update of the arrows
                        UpdateArrowsFromActionsQueue();
                        Debug.Log("[Combat] Updated arrows from queue after adding action");

                        gameMediator?.NotifyGameStateChanged();
                    } else {
                        Debug.LogWarning("[Combat] Failed to find either attacker or target creature in battlefield");
                    }
                    break;
                }
            }
        }
    }

    private ICreature FindCreatureByTargetId(CardController cardController) {
        if (cardController == null) {
            Debug.LogWarning("[FindCreature] CardController is null");
            return null;
        }

        string targetId = cardController.GetLinkedCreatureId();
        if (string.IsNullOrEmpty(targetId)) {
            Debug.LogWarning("[FindCreature] No linked creature ID found");
            return null;
        }

        Debug.Log($"[FindCreature] Looking for creature with TargetId: {targetId}");

        var player1Creature = gameManager.Player1.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
        if (player1Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player1's battlefield. TargetId: {player1Creature.TargetId}, Name: {player1Creature.Name}, Health: {player1Creature.Health}, Attack: {player1Creature.Attack}");
            return player1Creature;
        }

        var player2Creature = gameManager.Player2.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
        if (player2Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player2's battlefield. TargetId: {player2Creature.TargetId}, Name: {player2Creature.Name}, Health: {player2Creature.Health}, Attack: {player2Creature.Attack}");
            return player2Creature;
        }

        Debug.LogWarning($"[FindCreature] Could not find creature with TargetId {targetId} in either battlefield");
        return null;
    }

    private CardController FindCardControllerForCreature(ICreature creature) {
        if (creature == null) return null;
        Debug.Log($"[FindCardController] Looking for creature with TargetId: {creature.TargetId}");

        // First look in current battlefield
        var cardInCurrentBattlefield = creatureCards.Values.FirstOrDefault(card => {
            var linkedCreature = card.GetLinkedCreature();
            return linkedCreature != null && linkedCreature.TargetId == creature.TargetId;
        });

        if (cardInCurrentBattlefield != null) {
            Debug.Log($"[FindCardController] Found creature in current battlefield with TargetId: {creature.TargetId}");
            return cardInCurrentBattlefield;
        }

        // If not found, look in opponent's battlefield
        var otherBattlefield = player.IsPlayer1() ?
            gameReferences.GetPlayer2BattlefieldUI() :
            gameReferences.GetPlayer1BattlefieldUI();

        if (otherBattlefield != null) {
            var cardInOtherBattlefield = otherBattlefield.creatureCards.Values.FirstOrDefault(card => {
                var linkedCreature = card.GetLinkedCreature();
                return linkedCreature != null && linkedCreature.TargetId == creature.TargetId;
            });

            if (cardInOtherBattlefield != null) {
                Debug.Log($"[FindCardController] Found creature in opponent battlefield with TargetId: {creature.TargetId}");
                return cardInOtherBattlefield;
            }
        }

        Debug.LogWarning($"[FindCardController] Could not find card controller for creature with TargetId: {creature.TargetId}");
        return null;
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) {
            Debug.LogWarning("[BattlefieldUI] UpdateUI called but not initialized or player is null");
            return;
        }
        Debug.Log($"[BattlefieldUI] Updating UI for {(player.IsPlayer1() ? "Player 1" : "Player 2")}'s battlefield");

        UpdateArrowsFromActionsQueue();

        // Update existing cards first
        foreach (var creatureCard in creatureCards) {
            if (creatureCard.Value != null) {
                creatureCard.Value.UpdateUI();
            }
        }

        // Then handle any changes to the battlefield
        var currentCreatureIds = new HashSet<string>(player.Battlefield.Select(c => c.TargetId));
        var cardsToRemove = creatureCards.Keys.Where(id => !currentCreatureIds.Contains(id)).ToList();

        // Remove cards that are no longer on the battlefield
        foreach (var id in cardsToRemove) {
            if (creatureCards.TryGetValue(id, out var card)) {
                RemoveCard(card);
                if (card != null) {
                    Destroy(card.gameObject);
                }
                creatureCards.Remove(id);
            }
        }

        // Add new cards
        foreach (var creature in player.Battlefield) {
            if (!creatureCards.ContainsKey(creature.TargetId)) {
                var controller = CreateCreatureCard(creature);
                if (controller != null) {
                    creatureCards[creature.TargetId] = controller;
                    AddCard(controller);
                }
            }
        }

        dropZoneHandler?.ResetVisualFeedback();
    }

    private void UpdateArrowsFromActionsQueue() {
        Debug.Log("[UpdateArrowsFromActionsQueue] Starting update...");

        // Clear existing arrows
        foreach (var arrow in activeArrows.Values) {
            if (arrow != null) {
                Debug.Log($"[UpdateArrowsFromActionsQueue] Destroying existing arrow");
                Destroy(arrow.gameObject);
            }
        }
        activeArrows.Clear();

        if (gameManager.ActionsQueue == null) {
            Debug.LogWarning("[UpdateArrowsFromActionsQueue] ActionsQueue is null!");
            return;
        }

        var pendingActions = gameManager.ActionsQueue.GetPendingActions();
        Debug.Log($"[UpdateArrowsFromActionsQueue] Found {pendingActions.Count} pending actions");

        foreach (var action in pendingActions) {
            Debug.Log($"[UpdateArrowsFromActionsQueue] Processing action of type: {action.GetType().Name}");

            if (action is DamageCreatureAction damageAction) {
                var attacker = damageAction.GetAttacker();
                var target = damageAction.GetTarget();

                Debug.Log($"[UpdateArrowsFromActionsQueue] Found DamageCreatureAction - Attacker: {attacker?.Name}, Target: {target?.Name}");

                // Find the relevant card controllers
                var attackerCard = FindCardControllerForCreature(attacker);
                var targetCard = FindCardControllerForCreature(target);

                Debug.Log($"[UpdateArrowsFromActionsQueue] Found cards - Attacker: {(attackerCard != null)}, Target: {(targetCard != null)}");

                if (attackerCard != null && targetCard != null) {
                    // Create new arrow
                    var arrow = ArrowIndicator.Create(transform);
                    Debug.Log($"[UpdateArrowsFromActionsQueue] Created new ArrowIndicator");

                    // Get world positions of the cards
                    Vector3 startPos = attackerCard.transform.position;
                    Vector3 endPos = targetCard.transform.position;

                    // Ensure positions are on the same Z plane
                    startPos.z = 0;
                    endPos.z = 0;

                    arrow.Show(startPos, endPos);
                    Debug.Log($"[UpdateArrowsFromActionsQueue] Showed arrow from {startPos} to {endPos}");

                    string attackerId = attackerCard.GetCardData().cardName;
                    activeArrows[attackerId] = arrow;

                    Debug.Log($"[UpdateArrowsFromActionsQueue] Added arrow to activeArrows dictionary. Count: {activeArrows.Count}");
                } else {
                    Debug.LogWarning("[UpdateArrowsFromActionsQueue] Could not find either attacker or target card controller");
                }
            }
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
        }
    }

    private void OnCreatureDied(ICreature creature) {
    if (!IsInitialized) return;

    Debug.Log($"[BattlefieldUI] Handling creature death: {creature.Name}");
    if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
        Debug.Log($"[BattlefieldUI] Removing card for dead creature: {creature.Name}");
        RemoveCard(card);
        if (card != null) {
            Destroy(card.gameObject);
        }
        creatureCards.Remove(creature.TargetId);
    }

    // Force layout update
    UpdateLayout();
}

// Dragging functionality
protected override void OnCardBeginDrag(CardController card) {
    if (card == null) return;

    attackingCard = card;
    Vector3 startPos = card.transform.position;
    startPos.z = 0;
    dragArrowIndicator.Show(startPos, startPos);
    card.transform.SetAsLastSibling();

    Debug.Log($"Started dragging card at position: {startPos}");
}

public void OnCardDrag(PointerEventData eventData) {
    if (dragArrowIndicator != null && dragArrowIndicator.IsVisible()) {
        // Convert screen position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0; // Keep it on the same Z plane as the cards
        dragArrowIndicator.UpdateEndPosition(worldPos);

        Debug.Log($"Dragging arrow to position: {worldPos}");
    }
}

protected override void OnCardEndDrag(CardController card) {
    if (dragArrowIndicator != null) {
        dragArrowIndicator.Hide(); // Only hide the drag indicator
    }

    // Don't need to clear activeArrows here since they're managed by UpdateArrowsFromActionsQueue
    attackingCard = null;
    UpdateLayout();

    // Update the arrows based on the current state of the ActionsQueue
    UpdateArrowsFromActionsQueue();
}

// Empty implementations - no hover effects
protected override void OnCardHoverEnter(CardController card) { }
protected override void OnCardHoverExit(CardController card) { }

protected override void OnDestroy() {
    foreach (var arrow in activeArrows.Values) {
        if (arrow != null) {
            Destroy(arrow.gameObject);
        }
    }
    activeArrows.Clear();

    if (dragArrowIndicator != null) {
        Destroy(dragArrowIndicator.gameObject);
    }

    base.OnDestroy();
}
}