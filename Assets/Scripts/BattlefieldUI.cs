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

    // File: BattlefieldUI.cs

    private void HandleCreatureCombat(CardController attackingCard) {
        Debug.Log($"[Combat] Starting combat with attacker: {attackingCard.GetCardData().cardName}");

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

                        // Create damage action for the target creature only
                        var damageAction = new DamageCreatureAction(targetCreature, attackerCreature.Attack);
                        gameManager.ActionsQueue.AddAction(damageAction);

                        Debug.Log($"[Combat] Added damage action to queue. Queue size: {gameManager.ActionsQueue.GetPendingActionsCount()}");

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

        var cardName = cardController.GetComponent<CardController>().GetCardData().cardName;
        Debug.Log($"[FindCreature] Looking for creature with name: {cardName}");

        var player1Creature = gameManager.Player1.Battlefield.FirstOrDefault(c => c.Name == cardName);
        if (player1Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player1's battlefield. Name: {player1Creature.Name}, Health: {player1Creature.Health}, Attack: {player1Creature.Attack}");
            return player1Creature;
        }

        var player2Creature = gameManager.Player2.Battlefield.FirstOrDefault(c => c.Name == cardName);
        if (player2Creature != null) {
            Debug.Log($"[FindCreature] Found creature in Player2's battlefield. Name: {player2Creature.Name}, Health: {player2Creature.Health}, Attack: {player2Creature.Attack}");
            return player2Creature;
        }

        Debug.LogWarning($"[FindCreature] Could not find creature with name {cardName} in either battlefield");
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

    public override void UpdateUI() {
        if (!IsInitialized || player == null) {
            Debug.LogWarning("[BattlefieldUI] UpdateUI called but not initialized or player is null");
            return;
        }
        Debug.Log($"[BattlefieldUI] Updating UI for {(player.IsPlayer1() ? "Player 1" : "Player 2")}'s battlefield");

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