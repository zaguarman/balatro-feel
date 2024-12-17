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

    #region Initialization

    private void Start() {
        gameManager = GameManager.Instance;
        InitializeManagers();
        CreateSlots();
        Log("BattlefieldUI initialized", LogTag.Initialization);
    }

    private void InitializeManagers() {
        arrowManager = new BattlefieldArrowManager(transform, gameManager);
    }

    private void CreateSlots() {
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

        // Get the target slot using raycasting
        var targetSlot = GetTargetSlot();
        if (targetSlot == null) return;

        // Check if the card originated from HandUI
        bool isFromHand = IsCardFromHand(card);

        if (isFromHand) {
            HandleCardFromHand(card, targetSlot);
        } else {
            HandleCardFromBattlefield(card, targetSlot);
        }

        arrowManager.UpdateArrowsFromActionsQueue();
        gameMediator?.NotifyGameStateChanged();
    }

    private bool IsCardFromHand(CardController card) {
        // Check if the original parent is part of a HandUI
        if (card.OriginalParent == null) return false;

        Transform parent = card.OriginalParent;
        while (parent != null) {
            if (parent.GetComponent<HandUI>() != null) {
                return true;
            }
            parent = parent.parent;
        }
        return false;
    }

    private CardContainer GetSourceContainer(CardController card) {
        if (card == null) return null;

        // Use the original parent stored during drag start
        var originalParent = card.OriginalParent;
        if (originalParent == null) {
            Log($"Original parent is null for card: {card.name}", LogTag.UI | LogTag.Cards);
            return null;
        }

        // The card's original parent should be a slot, so we need to get its parent which would be the container
        var containerObject = originalParent.parent?.gameObject;
        if (containerObject == null) {
            Log($"Container object is null for card: {card.name}", LogTag.UI | LogTag.Cards);
            return null;
        }

        var container = containerObject.GetComponent<CardContainer>();
        Log($"Found source container: {(container != null ? container.GetType().Name : "null")}", LogTag.UI | LogTag.Cards);
        return container;
    }

    private void HandleCardFromHand(CardController card, BattlefieldSlot targetSlot) {
        var cardData = card.GetCardData();
        if (cardData != null) {
            var newCard = CardFactory.CreateCard(cardData);
            if (newCard != null) {
                int slotIndex = targetSlot != null ? targetSlot.Index : -1;
                Log($"Adding PlayCardAction for {cardData.cardName} to slot {slotIndex}", LogTag.Actions | LogTag.Cards);
                gameManager.ActionsQueue.AddAction(new PlayCardAction(newCard, player, slotIndex));
            }
        }
    }

    private void HandleCardFromBattlefield(CardController card, BattlefieldSlot targetSlot) {
        if (targetSlot != null && targetSlot.IsOccupied) {
            var targetCard = targetSlot.OccupyingCard;
            if (card.IsPlayer1Card() == targetCard.IsPlayer1Card()) {
                // Same player creatures - handle swap
                HandleCreatureSwap(card, targetSlot);
            } else {
                // Enemy creature - handle combat
                gameManager.CombatHandler.HandleCreatureCombat(card);
            }
        } else {
            gameManager.CombatHandler.HandleCreatureCombat(card);
        }
    }

    private BattlefieldSlot GetTargetSlot() {
        // First try to find the slot using EventSystem raycast
        var pointerEventData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        Log($"UI Raycast found {raycastResults.Count} hits", LogTag.UI);

        // First pass: Look for direct BattlefieldSlot components
        foreach (var result in raycastResults) {
            Log($"Hit UI object: {result.gameObject.name}", LogTag.UI);

            var slot = result.gameObject.GetComponent<BattlefieldSlot>();
            if (slot != null) {
                Log($"Found direct slot: {slot.gameObject.name}", LogTag.UI);
                return slot;
            }
        }

        // Second pass: Look for parent BattlefieldSlot components
        foreach (var result in raycastResults) {
            var parentSlot = result.gameObject.GetComponentInParent<BattlefieldSlot>();
            if (parentSlot != null) {
                Log($"Found parent slot: {parentSlot.gameObject.name}", LogTag.UI);
                return parentSlot;
            }
        }

        // If no slot found through raycast, try to find the nearest slot based on position
        if (slots != null && slots.Count > 0) {
            Vector2 mousePos = Input.mousePosition;
            float closestDistance = float.MaxValue;
            BattlefieldSlot nearestSlot = null;

            foreach (var slot in slots) {
                if (slot == null) continue;

                Vector2 slotScreenPos = Camera.main.WorldToScreenPoint(slot.transform.position);
                float distance = Vector2.Distance(mousePos, slotScreenPos);

                if (distance < closestDistance) {
                    closestDistance = distance;
                    nearestSlot = slot;
                }
            }

            if (nearestSlot != null) {
                Log($"Found nearest slot: {nearestSlot.gameObject.name}", LogTag.UI);
                return nearestSlot;
            }
        }

        Log("No valid slot found", LogTag.UI);
        return null;
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

    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}