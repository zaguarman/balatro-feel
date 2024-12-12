using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static DebugLogger;

public class BattlefieldUI : CardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();
    protected GameManager gameManager;
    private CardController attackingCard;

    private BattlefieldArrowManager arrowManager;
    private BattlefieldCombatHandler combatHandler;

    private void Start() {
        gameManager = GameManager.Instance;
        InitializeManagers();
        Log("BattlefieldUI initialized", LogTag.Initialization);
    }

    public override void Initialize(IPlayer player) {
        base.Initialize(player);

        if (gameManager?.ActionsQueue != null) {
            gameManager.ActionsQueue.OnActionsResolved += OnActionsResolved;
        }
    }

    private void OnActionsResolved() {
        combatHandler?.ResetAttackingCreatures();
    }

    public CardController GetCardControllerByCreatureId(string creatureId) {
        if (string.IsNullOrEmpty(creatureId)) return null;

        creatureCards.TryGetValue(creatureId, out var cardController);
        return cardController;
    }

    private void InitializeManagers() {
        arrowManager = new BattlefieldArrowManager(transform, gameManager);
        combatHandler = new BattlefieldCombatHandler(gameManager);
    }

    protected override void HandleCardDropped(CardController card) {
        if (card == null || !CanAcceptCard(card)) return;
        if (gameManager == null) return;

        if (card.transform.parent.GetComponent<HandUI>() != null) {
            HandleNewCreature(card);
        } else if (card.transform.parent.GetComponent<BattlefieldUI>() != null) {
            combatHandler.HandleCreatureCombat(card);
            arrowManager.UpdateArrowsFromActionsQueue();
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private void HandleNewCreature(CardController card) {
        if (card.GetCardData() is CreatureData creatureData) {
            ICard newCard = CardFactory.CreateCard(creatureData);
            if (newCard != null) {
                newCard.Play(
                    card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2,
                    gameManager.ActionsQueue
                );
                gameMediator?.NotifyGameStateChanged();
            }
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;

        arrowManager.UpdateArrowsFromActionsQueue();
        UpdateCreatureCards();
        dropZoneHandler?.ResetVisualFeedback();
    }

    public void ResetAttackingCreatures() {
        if (combatHandler != null) {
            combatHandler.ResetAttackingCreatures();
        }
    }

    private void UpdateCreatureCards() {
        // Update existing cards
        foreach (var card in creatureCards.Values) {
            if (card != null) card.UpdateUI();
        }

        // Handle battlefield changes
        UpdateBattlefieldChanges();
    }

    private void UpdateBattlefieldChanges() {
        var currentCreatureIds = new HashSet<string>(player.Battlefield.Select(c => c.TargetId));
        RemoveDeadCreatures(currentCreatureIds);
        AddNewCreatures(currentCreatureIds);
    }

    private void RemoveDeadCreatures(HashSet<string> currentCreatureIds) {
        var cardsToRemove = creatureCards.Keys.Where(id => !currentCreatureIds.Contains(id)).ToList();
        foreach (var id in cardsToRemove) {
            if (creatureCards.TryGetValue(id, out var card)) {
                RemoveCard(card);
                if (card != null) Destroy(card.gameObject);
                creatureCards.Remove(id);
            }
        }
    }

    private void AddNewCreatures(HashSet<string> currentCreatureIds) {
        foreach (var creature in player.Battlefield) {
            if (!creatureCards.ContainsKey(creature.TargetId)) {
                var controller = CreateCreatureCard(creature);
                if (controller != null) {
                    creatureCards[creature.TargetId] = controller;
                    AddCard(controller);
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

        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            RemoveCard(card);
            if (card != null) Destroy(card.gameObject);
            creatureCards.Remove(creature.TargetId);
        }

        UpdateLayout();
    }

    // Dragging functionality
    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;

        attackingCard = card;
        arrowManager.ShowDragArrow(card.transform.position);
        card.transform.SetAsLastSibling();
    }

    public void OnCardDrag(PointerEventData eventData) {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        arrowManager.UpdateDragArrow(worldPos);
    }

    protected override void OnCardEndDrag(CardController card) {
        arrowManager.HideDragArrow();
        attackingCard = null;
        UpdateLayout();
        arrowManager.UpdateArrowsFromActionsQueue();
    }

    // Empty implementations - no hover effects
    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }

    protected override void OnDestroy() {
        if (gameManager?.ActionsQueue != null) {
            gameManager.ActionsQueue.OnActionsResolved -= OnActionsResolved;
        }
        base.OnDestroy();
    }
}