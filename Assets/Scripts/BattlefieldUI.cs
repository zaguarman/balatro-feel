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

    private const LogTag INITIALIZATION_LOG_TAG = LogTag.Initialization;
    private const LogTag CREATURE_LOG_TAG = LogTag.Creatures;
    private const LogTag ACTIONS_LOG_TAG = LogTag.Actions;
    private const LogTag CARD_LOG_TAG = LogTag.Cards;

    private void Start() {
        gameManager = GameManager.Instance;
        SetupDragArrow();
        DebugLogger.Log("BattlefieldUI initialized", INITIALIZATION_LOG_TAG);
    }

    private void SetupDragArrow() {
        dragArrowIndicator = ArrowIndicator.Create(transform);
        dragArrowIndicator.Hide();
        DebugLogger.Log("Drag arrow indicator setup completed", INITIALIZATION_LOG_TAG);
    }

    protected override void HandleCardDropped(CardController card) {
        if (card == null || !CanAcceptCard(card)) {
            DebugLogger.Log("Card dropped is null or cannot be accepted", CARD_LOG_TAG);
            return;
        }

        if (gameManager == null) {
            DebugLogger.LogError("GameManager reference is null", INITIALIZATION_LOG_TAG);
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
            var gameManager = GameManager.Instance;
            if (gameManager != null) {
                DebugLogger.Log($"Creating new creature from {creatureData.cardName} with {creatureData.effects.Count} effects",
                    CREATURE_LOG_TAG | ACTIONS_LOG_TAG);

                // Create the card through CardFactory to ensure effects are copied
                ICard newCard = CardFactory.CreateCard(creatureData);
                if (newCard != null) {
                    DebugLogger.Log($"Playing creature with {newCard.Effects.Count} effects", CREATURE_LOG_TAG | ACTIONS_LOG_TAG);
                    newCard.Play(card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2, gameManager.ActionsQueue);
                    gameMediator?.NotifyGameStateChanged();
                }
            }
        }
    }

    private void HandleCreatureCombat(CardController attackingCard) {
        DebugLogger.Log($"Starting combat with attacker: {attackingCard.GetCardData().cardName}",
            ACTIONS_LOG_TAG | CREATURE_LOG_TAG);

        // Convert mouse position to world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Ensure we're on the same Z plane as the cards

        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        DebugLogger.Log($"Found {raycastResults.Count} potential targets under mouse position", ACTIONS_LOG_TAG);

        foreach (var result in raycastResults) {
            var targetCardController = result.gameObject.GetComponent<CardController>();
            if (targetCardController != null && targetCardController != attackingCard) {
                DebugLogger.Log($"Found target card: {targetCardController.GetCardData().cardName}", ACTIONS_LOG_TAG);

                // Ensure the target is an enemy creature
                bool isEnemyCreature = targetCardController.IsPlayer1Card() != attackingCard.IsPlayer1Card();
                DebugLogger.Log($"Is enemy creature? {isEnemyCreature}", ACTIONS_LOG_TAG);

                if (isEnemyCreature) {
                    // Get the actual creatures from the battlefield
                    var attackerCreature = FindCreatureByTargetId(attackingCard);
                    var targetCreature = FindCreatureByTargetId(targetCardController);

                    DebugLogger.Log($"Found attacker creature: {(attackerCreature != null ? attackerCreature.Name : "null")}",
                        CREATURE_LOG_TAG | ACTIONS_LOG_TAG);
                    DebugLogger.Log($"Found target creature: {(targetCreature != null ? targetCreature.Name : "null")}",
                        CREATURE_LOG_TAG | ACTIONS_LOG_TAG);

                    if (attackerCreature != null && targetCreature != null) {
                        DebugLogger.Log($"Creating DamageCreatureAction - Attacker: {attackerCreature.Name}, Attack: {attackerCreature.Attack}, Target: {targetCreature.Name}, Current Health: {targetCreature.Health}",
                            ACTIONS_LOG_TAG | CREATURE_LOG_TAG);

                        // Create damage action including the attacker reference
                        var damageAction = new DamageCreatureAction(targetCreature, attackerCreature.Attack, attackerCreature);
                        gameManager.ActionsQueue.AddAction(damageAction);

                        DebugLogger.Log($"Added damage action to queue. Queue size: {gameManager.ActionsQueue.GetPendingActionsCount()}",
                            ACTIONS_LOG_TAG);

                        // Force an immediate update of the arrows
                        UpdateArrowsFromActionsQueue();
                        DebugLogger.Log("Updated arrows from queue after adding action", ACTIONS_LOG_TAG);

                        gameMediator?.NotifyGameStateChanged();
                    } else {
                        DebugLogger.LogWarning("Failed to find either attacker or target creature in battlefield",
                            CREATURE_LOG_TAG | ACTIONS_LOG_TAG);
                    }
                    break;
                }
            }
        }
    }

    private ICreature FindCreatureByTargetId(CardController cardController) {
        if (cardController == null) {
            DebugLogger.LogWarning("CardController is null", CREATURE_LOG_TAG);
            return null;
        }

        string targetId = cardController.GetLinkedCreatureId();
        if (string.IsNullOrEmpty(targetId)) {
            DebugLogger.LogWarning("No linked creature ID found", CREATURE_LOG_TAG);
            return null;
        }

        DebugLogger.Log($"Looking for creature with TargetId: {targetId}", CREATURE_LOG_TAG);

        var player1Creature = gameManager.Player1.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
        if (player1Creature != null) {
            DebugLogger.Log($"Found creature in Player1's battlefield. TargetId: {player1Creature.TargetId}, Name: {player1Creature.Name}, Health: {player1Creature.Health}, Attack: {player1Creature.Attack}",
                CREATURE_LOG_TAG);
            return player1Creature;
        }

        var player2Creature = gameManager.Player2.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
        if (player2Creature != null) {
            DebugLogger.Log($"Found creature in Player2's battlefield. TargetId: {player2Creature.TargetId}, Name: {player2Creature.Name}, Health: {player2Creature.Health}, Attack: {player2Creature.Attack}",
                CREATURE_LOG_TAG);
            return player2Creature;
        }

        DebugLogger.LogWarning($"Could not find creature with TargetId {targetId} in either battlefield", CREATURE_LOG_TAG);
        return null;
    }

    private CardController FindCardControllerForCreature(ICreature creature) {
        if (creature == null) return null;
        DebugLogger.Log($"Looking for creature with TargetId: {creature.TargetId}", CREATURE_LOG_TAG);

        // First look in current battlefield
        var cardInCurrentBattlefield = creatureCards.Values.FirstOrDefault(card => {
            var linkedCreature = card.GetLinkedCreature();
            return linkedCreature != null && linkedCreature.TargetId == creature.TargetId;
        });

        if (cardInCurrentBattlefield != null) {
            DebugLogger.Log($"Found creature in current battlefield with TargetId: {creature.TargetId}", CREATURE_LOG_TAG);
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
                DebugLogger.Log($"Found creature in opponent battlefield with TargetId: {creature.TargetId}", CREATURE_LOG_TAG);
                return cardInOtherBattlefield;
            }
        }

        DebugLogger.LogWarning($"Could not find card controller for creature with TargetId: {creature.TargetId}", CREATURE_LOG_TAG);
        return null;
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) {
            DebugLogger.LogWarning("UpdateUI called but not initialized or player is null", INITIALIZATION_LOG_TAG);
            return;
        }
        DebugLogger.Log($"Updating UI for {(player.IsPlayer1() ? "Player 1" : "Player 2")}'s battlefield", CREATURE_LOG_TAG);

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
        DebugLogger.Log("Starting update of arrows from actions queue", ACTIONS_LOG_TAG);

        // Clear existing arrows
        foreach (var arrow in activeArrows.Values) {
            if (arrow != null) {
                DebugLogger.Log("Destroying existing arrow", ACTIONS_LOG_TAG);
                Destroy(arrow.gameObject);
            }
        }
        activeArrows.Clear();

        if (gameManager.ActionsQueue == null) {
            DebugLogger.LogWarning("ActionsQueue is null!", ACTIONS_LOG_TAG);
            return;
        }

        var pendingActions = gameManager.ActionsQueue.GetPendingActions();
        DebugLogger.Log($"Found {pendingActions.Count} pending actions", ACTIONS_LOG_TAG);

        foreach (var action in pendingActions) {
            DebugLogger.Log($"Processing action of type: {action.GetType().Name}", ACTIONS_LOG_TAG);

            if (action is DamageCreatureAction damageAction) {
                var attacker = damageAction.GetAttacker();
                var target = damageAction.GetTarget();

                DebugLogger.Log($"Found DamageCreatureAction - Attacker: {attacker?.Name}, Target: {target?.Name}",
                    ACTIONS_LOG_TAG | CREATURE_LOG_TAG);

                // Find the relevant card controllers
                var attackerCard = FindCardControllerForCreature(attacker);
                var targetCard = FindCardControllerForCreature(target);

                DebugLogger.Log($"Found cards - Attacker: {(attackerCard != null)}, Target: {(targetCard != null)}",
                    ACTIONS_LOG_TAG | CREATURE_LOG_TAG);

                if (attackerCard != null && targetCard != null) {
                    // Create new arrow
                    var arrow = ArrowIndicator.Create(transform);
                    DebugLogger.Log("Created new ArrowIndicator", ACTIONS_LOG_TAG);

                    // Get world positions of the cards
                    Vector3 startPos = attackerCard.transform.position;
                    Vector3 endPos = targetCard.transform.position;

                    // Ensure positions are on the same Z plane
                    startPos.z = 0;
                    endPos.z = 0;

                    arrow.Show(startPos, endPos);
                    DebugLogger.Log($"Showed arrow from {startPos} to {endPos}", ACTIONS_LOG_TAG);

                    string attackerId = attackerCard.GetCardData().cardName;
                    activeArrows[attackerId] = arrow;

                    DebugLogger.Log($"Added arrow to activeArrows dictionary. Count: {activeArrows.Count}", ACTIONS_LOG_TAG);
                } else {
                    DebugLogger.LogWarning("Could not find either attacker or target card controller",
                        ACTIONS_LOG_TAG | CREATURE_LOG_TAG);
                }
            }
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
            DebugLogger.Log("Registered game events", INITIALIZATION_LOG_TAG);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
            DebugLogger.Log("Unregistered game events", INITIALIZATION_LOG_TAG);
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (!IsInitialized) return;

        DebugLogger.Log($"Handling creature death: {creature.Name}", CREATURE_LOG_TAG);
        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            DebugLogger.Log($"Removing card for dead creature: {creature.Name}", CREATURE_LOG_TAG);
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

        DebugLogger.Log($"Started dragging card at position: {startPos}", CARD_LOG_TAG);
    }

    public void OnCardDrag(PointerEventData eventData) {
        if (dragArrowIndicator != null && dragArrowIndicator.IsVisible()) {
            // Convert screen position to world position
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0; // Keep it on the same Z plane as the cards
            dragArrowIndicator.UpdateEndPosition(worldPos);

            DebugLogger.Log($"Dragging arrow to position: {worldPos}", CARD_LOG_TAG);
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