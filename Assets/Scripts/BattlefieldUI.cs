using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static DebugLogger;

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

        var targetSlot = GetTargetSlot();
        if (targetSlot == null) return;

        if (IsCardFromHand(card)) {
            HandleCardFromHand(card, targetSlot);
        } else {
            HandleCardFromBattlefield(card, targetSlot);
        }

        arrowManager.UpdateArrowsFromActionsQueue();
        gameMediator?.NotifyGameStateChanged();
    }

    private bool IsCardFromHand(CardController card) {
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
        if (targetSlot != null) {
            if (card.IsPlayer1Card() != player.IsPlayer1()) {
                // Enemy slot - handle combat
                gameManager.CombatHandler.HandleCreatureCombat(card, targetSlot);
            } else {
                // Same player slot - handle swap
                HandleCreatureSwap(card, targetSlot);
            }
        }
    }

    private void HandleCreatureSwap(CardController card1, BattlefieldSlot targetSlot) {
        var creature1 = FindCreatureByTargetId(card1);
        var creature2 = FindCreatureByTargetId(targetSlot.OccupyingCard);

        if (creature1 == null || creature2 == null) return;

        int slot1Index = slots.FindIndex(s => s.OccupyingCard == card1);
        int slot2Index = targetSlot.Index;

        if (slot1Index != -1 && slot2Index != -1) {
            Log($"Creating swap action between {creature1.Name} (slot {slot1Index}) and {creature2.Name} (slot {slot2Index})",
                LogTag.Actions | LogTag.Creatures);
            var swapAction = new SwapCreaturesAction(creature1, creature2, slot1Index, slot2Index, player);
            gameManager.ActionsQueue.AddAction(swapAction);
        }
    }

    private BattlefieldSlot GetTargetSlot() {
        var pointerEventData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        Log($"UI Raycast found {raycastResults.Count} hits", LogTag.UI);

        foreach (var result in raycastResults) {
            var slot = result.gameObject.GetComponent<BattlefieldSlot>();
            if (slot != null) {
                Log($"Found slot: {slot.gameObject.name}", LogTag.UI);
                return slot;
            }

            var parentSlot = result.gameObject.GetComponentInParent<BattlefieldSlot>();
            if (parentSlot != null) {
                Log($"Found parent slot: {parentSlot.gameObject.name}", LogTag.UI);
                return parentSlot;
            }
        }

        // If no slot found through raycast, find nearest slot
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

    private ICreature FindCreatureByTargetId(CardController cardController) {
        if (cardController == null) return null;

        string targetId = cardController.GetLinkedCreatureId();
        if (string.IsNullOrEmpty(targetId)) {
            LogWarning($"No linked creature ID found for card {cardController.GetCardData()?.cardName}", LogTag.Cards | LogTag.Creatures);
            return null;
        }

        var creature = player?.Battlefield.FirstOrDefault(c => c.TargetId == targetId);
        if (creature != null) return creature;

        creature = gameManager?.Player1.Battlefield.FirstOrDefault(c => c.TargetId == targetId) ??
                  gameManager?.Player2.Battlefield.FirstOrDefault(c => c.TargetId == targetId);

        if (creature == null) {
            LogWarning($"Could not find creature with ID {targetId}", LogTag.Cards | LogTag.Creatures);
        }

        return creature;
    }

    #endregion

    #region UI Updates

    public override void UpdateUI() {
        if (!IsInitialized || player == null) return;

        UpdateCreatureCards();
        UpdateSlotOccupancy();
        arrowManager.UpdateArrowsFromActionsQueue();
    }

    private void UpdateCreatureCards() {
        foreach (var slot in slots) {
            slot.ClearSlot();
        }

        var currentCreatureIds = new HashSet<string>(player.Battlefield.Select(c => c.TargetId));
        var cardsToRemove = creatureCards.Keys.Where(id => !currentCreatureIds.Contains(id)).ToList();

        foreach (var id in cardsToRemove) {
            if (creatureCards.TryGetValue(id, out var card)) {
                if (card != null) Destroy(card.gameObject);
                creatureCards.Remove(id);
            }
        }

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

    #endregion

    #region Event Handling

    public override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
        }
    }

    public override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
        }
    }

    public override void OnCreatureDied(ICreature creature) {
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

    public override void OnCardBeginDrag(CardController card) {
        if (card == null) return;

        var startSlot = slots.FirstOrDefault(s => s.OccupyingCard == card);
        if (startSlot != null) {
            arrowManager.ShowDragArrow(startSlot.transform.position);
        } else {
            arrowManager.ShowDragArrow(card.transform.position);
        }
        card.transform.SetAsLastSibling();
    }

    public override void OnCardEndDrag(CardController card) {
        arrowManager.HideDragArrow();
        UpdateUI();
    }

    public void OnCardDrag(PointerEventData eventData) {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        arrowManager.UpdateDragArrow(worldPos);
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

    public CardController GetCardControllerByCreatureId(string creatureId) {
        if (string.IsNullOrEmpty(creatureId)) return null;
        creatureCards.TryGetValue(creatureId, out var cardController);
        return cardController;
    }

    protected override void OnCardHoverEnter(CardController card) { }
    protected override void OnCardHoverExit(CardController card) { }
}