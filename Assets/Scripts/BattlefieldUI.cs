using static DebugLogger;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine;

public class BattlefieldUI : CardContainer {
    private const int MAX_SLOTS = 5;
    private readonly List<BattlefieldSlot> slots = new List<BattlefieldSlot>();
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();

    protected GameManager gameManager;
    private BattlefieldArrowManager arrowManager;
    private BattlefieldCombatHandler combatHandler;

    #region Initialization

    private void Start() {
        gameManager = GameManager.Instance;
        InitializeManagers();
        CreateSlots();
        Log("BattlefieldUI initialized", LogTag.Initialization);
    }

    private void InitializeManagers() {
        arrowManager = new BattlefieldArrowManager(transform, gameManager);
        combatHandler = new BattlefieldCombatHandler(gameManager);
    }

    private void CreateSlots() {
        // Create slot GameObjects
        for (int i = 0; i < MAX_SLOTS; i++) {
            GameObject slotObj = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotObj.transform.SetParent(transform, false);

            var slot = slotObj.AddComponent<BattlefieldSlot>();
            slot.Initialize(i, defaultColor, validDropColor, invalidDropColor, hoverColor);
            slots.Add(slot);
        }
        UpdateSlotPositions();
    }

    private void UpdateSlotPositions() {
        float totalWidth = (MAX_SLOTS - 1) * settings.spacing;
        float startX = -totalWidth / 2;

        for (int i = 0; i < slots.Count; i++) {
            float xPos = startX + (settings.spacing * i);
            slots[i].SetPosition(new Vector2(xPos, 0));
        }
    }

    #endregion

    #region Card Handling

    protected override void HandleCardDropped(CardController card) {
        if (card == null || !CanAcceptCard(card)) return;
        if (gameManager == null) return;

        var sourceContainer = card.transform.parent.GetComponent<CardContainer>();
        var targetSlot = GetTargetSlot(Input.mousePosition);

        if (sourceContainer is HandUI) {
            // Create and queue the play card action for hand drops
            var cardData = card.GetCardData();
            if (cardData != null) {
                var newCard = CardFactory.CreateCard(cardData);
                if (newCard != null) {
                    gameManager.ActionsQueue.AddAction(new PlayCardAction(newCard, player, targetSlot?.Index ?? -1));
                    gameMediator?.NotifyGameStateChanged();
                }
            }
        } else if (sourceContainer is BattlefieldUI) {
            if (targetSlot != null && targetSlot.IsOccupied) {
                var targetCard = targetSlot.OccupyingCard;
                if (card.IsPlayer1Card() == targetCard.IsPlayer1Card()) {
                    // Same player creatures - handle swap
                    HandleCreatureSwap(card, targetSlot);
                } else {
                    // Enemy creature - handle combat
                    combatHandler.HandleCreatureCombat(card);
                }
            } else {
                combatHandler.HandleCreatureCombat(card);
            }
            arrowManager.UpdateArrowsFromActionsQueue();
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private BattlefieldSlot GetTargetSlot(Vector3 mousePosition) {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

        foreach (var hit in hits) {
            var slot = hit.collider?.GetComponent<BattlefieldSlot>();
            if (slot != null) return slot;
        }

        return null;
    }

    private void HandleNewCreature(CardController card, int slotIndex) {
        if (card.GetCardData() is CreatureData creatureData) {
            ICard newCard = CardFactory.CreateCard(creatureData);
            if (newCard != null) {
                ((Player)player).AddToBattlefield(newCard as ICreature, slotIndex);
                gameMediator?.NotifyGameStateChanged();
            }
        }
    }

    private void HandleCreatureSwap(CardController card1, BattlefieldSlot targetSlot) {
        var creature1 = FindCreatureByTargetId(card1);
        var creature2 = FindCreatureByTargetId(targetSlot.OccupyingCard);

        if (creature1 == null || creature2 == null) return;

        // Find the source slot index
        int slot1Index = slots.FindIndex(s => s.OccupyingCard == card1);
        int slot2Index = targetSlot.Index;

        // Only create swap action if we found both slot indices
        if (slot1Index != -1 && slot2Index != -1) {
            Log($"Creating swap action between {creature1.Name} (slot {slot1Index}) and {creature2.Name} (slot {slot2Index})",
                LogTag.Actions | LogTag.Creatures);
            var swapAction = new SwapCreaturesAction(creature1, creature2, slot1Index, slot2Index, player);
            gameManager.ActionsQueue.AddAction(swapAction);
        } else {
            LogWarning($"Could not find slot indices for swap. Slot1: {slot1Index}, Slot2: {slot2Index}",
                LogTag.Actions | LogTag.Creatures);
        }
    }

    private ICreature FindCreatureByTargetId(CardController cardController) {
        if (cardController == null) return null;

        string targetId = cardController.GetLinkedCreatureId();
        return player.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
    }

    #endregion

    #region UI Updates

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;

        arrowManager.UpdateArrowsFromActionsQueue();
        UpdateCreatureCards();
        UpdateSlotOccupancy();
    }

    private void UpdateCreatureCards() {
        // Clear all slots first
        foreach (var slot in slots) {
            slot.ClearSlot();
        }

        // Remove cards that are no longer on the battlefield
        var currentCreatureIds = new HashSet<string>(player.Battlefield.Select(c => c.TargetId));
        var cardsToRemove = creatureCards.Keys.Where(id => !currentCreatureIds.Contains(id)).ToList();
        foreach (var id in cardsToRemove) {
            if (creatureCards.TryGetValue(id, out var card)) {
                if (card != null) Destroy(card.gameObject);
                creatureCards.Remove(id);
            }
        }

        // Update or create cards for creatures
        for (int i = 0; i < player.Battlefield.Count; i++) {
            var creature = player.Battlefield[i];
            if (!creatureCards.TryGetValue(creature.TargetId, out var cardController)) {
                cardController = CreateCreatureCard(creature);
                if (cardController != null) {
                    creatureCards[creature.TargetId] = cardController;
                }
            }

            if (cardController != null) {
                // Find the appropriate slot
                var slot = slots[i];
                slot.OccupySlot(cardController);
                cardController.UpdateUI();
            }
        }
    }

    private void UpdateSlotOccupancy() {
        foreach (var slot in slots) {
            slot.ResetVisuals();
        }
    }

    public CardController GetCardControllerByCreatureId(string creatureId) {
        if (string.IsNullOrEmpty(creatureId)) return null;
        creatureCards.TryGetValue(creatureId, out var cardController);
        return cardController;
    }

    #endregion

    #region Event Handling

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
            var slot = slots.Find(s => s.OccupyingCard == card);
            if (slot != null) {
                slot.ClearSlot();
            }

            if (card != null) {
                Destroy(card.gameObject);
            }
            creatureCards.Remove(creature.TargetId);
        }

        UpdateUI();
    }

    #endregion

    #region Drag Handling

    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;

        arrowManager.ShowDragArrow(card.transform.position);
        card.transform.SetAsLastSibling();
    }

    public void OnCardDrag(PointerEventData eventData) {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        arrowManager.UpdateDragArrow(worldPos);
    }

    protected override void OnCardEndDrag(CardController card) {
        arrowManager.HideDragArrow();
        UpdateUI();
        arrowManager.UpdateArrowsFromActionsQueue();
    }

    #endregion

    #region Cleanup

    protected override void OnDestroy() {
        base.OnDestroy();
        foreach (var slot in slots) {
            if (slot != null) {
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();

        foreach (var card in creatureCards.Values) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        creatureCards.Clear();

        if (arrowManager != null) {
            arrowManager.Cleanup();
        }
    }

    #endregion

    // Empty implementations for hover effects (can be implemented later if needed)
    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}