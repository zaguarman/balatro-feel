using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlefieldUI : CardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();
    protected GameManager gameManager;

    private void Start() {
        gameManager = GameManager.Instance;
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

    protected void HandleCreatureCombat(CardController attackingCard) {
        Debug.Log($"[Combat] Starting combat with attacker: {attackingCard.GetCardData().cardName}");

        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (var result in raycastResults) {
            var targetCardController = result.gameObject.GetComponent<CardController>();
            if (targetCardController != null && targetCardController != attackingCard) {
                // Ensure the target is an enemy creature
                bool isEnemyCreature = targetCardController.IsPlayer1Card() != attackingCard.IsPlayer1Card();

                if (isEnemyCreature) {
                    var attackerCreature = FindCreatureByTargetId(attackingCard);
                    var targetCreature = FindCreatureByTargetId(targetCardController);

                    if (attackerCreature != null && targetCreature != null) {
                        Debug.Log($"[Combat] Creating DamageCreatureAction - Attacker: {attackerCreature.Name} ({attackerCreature.TargetId}), Target: {targetCreature.Name} ({targetCreature.TargetId})");

                        var damageAction = new DamageCreatureAction(targetCreature, attackerCreature.Attack);
                        gameManager.ActionsQueue.AddAction(damageAction);
                        gameMediator?.NotifyGameStateChanged();
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

        string creatureId = cardController.GetLinkedCreatureId();
        if (string.IsNullOrEmpty(creatureId)) {
            Debug.LogWarning("[FindCreature] No linked creature ID found");
            return null;
        }

        // Look for the creature in both battlefields using the TargetId
        var player1Creature = gameManager.Player1.Battlefield.Find(c => c.TargetId == creatureId);
        if (player1Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player1's battlefield. ID: {creatureId}, Name: {player1Creature.Name}");
            return player1Creature;
        }

        var player2Creature = gameManager.Player2.Battlefield.Find(c => c.TargetId == creatureId);
        if (player2Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player2's battlefield. ID: {creatureId}, Name: {player2Creature.Name}");
            return player2Creature;
        }

        Debug.LogWarning($"[FindCreature] Could not find creature with ID {creatureId} in either battlefield");
        return null;
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

        Debug.Log($"[BattlefieldUI] Handling creature death: {creature.Name} (ID: {creature.TargetId})");
        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            Debug.Log($"[BattlefieldUI] Removing card for dead creature: {creature.Name} (ID: {creature.TargetId})");
            RemoveCard(card);
            if (card != null) {
                Destroy(card.gameObject);
            }
            creatureCards.Remove(creature.TargetId);
        }

        UpdateLayout();
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) {
            Debug.LogWarning("[BattlefieldUI] UpdateUI called but not initialized or player is null");
            return;
        }

        // Update existing cards first
        foreach (var creatureCard in creatureCards) {
            if (creatureCard.Value != null) {
                creatureCard.Value.UpdateUI();
            }
        }

        // Handle changes to the battlefield
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

    // Basic drag functionality for combat
    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;
        card.transform.SetAsLastSibling();
    }

    protected override void OnCardEndDrag(CardController card) {
        if (card == null) return;
        UpdateLayout();
    }

    // Empty implementations - no hover effects
    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}