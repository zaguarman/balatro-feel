using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattlefieldUI : BaseCardContainer {
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();

    public override void Initialize(CardContainer battlefield, IPlayer player) {
        if (battlefield == null) {
            Debug.LogError("Battlefield container is null in BattlefieldUI.Initialize");
            return;
        }

        base.Initialize(battlefield, player);
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
            gameMediator.AddGameInitializedListener(OnGameInitialized);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
            gameMediator.RemoveGameInitializedListener(OnGameInitialized);
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null || container == null) return;
        UpdateBattlefieldCards();
    }

    protected override void UpdateContainerSettings() {
        base.UpdateContainerSettings();

        // Configure the drop zone
        var dropZone = container.gameObject.GetComponent<DebugDropZone>();
        if (dropZone == null) {
            dropZone = container.gameObject.AddComponent<DebugDropZone>();
        }
    }

    private void UpdateBattlefieldCards() {
        // Clear existing cards
        foreach (var existingCard in creatureCards.Values) {
            if (existingCard != null) {
                Destroy(existingCard.gameObject);
            }
        }
        creatureCards.Clear();
        cards.Clear();

        // Create new cards for each creature
        foreach (var creature in player.Battlefield) {
            CreateCreatureCard(creature);
        }

        // Update container size and card positions
        UpdateContainerSize(player.Battlefield.Count);

        // Make sure container layout is updated
        if (container != null) {
            container.UpdateUI();

            // Force an immediate layout update
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
    }

    private void CreateCreatureCard(ICreature creature) {
        var controller = CreateCard(creature, container.transform);
        if (controller != null) {
            creatureCards[creature.TargetId] = controller;
            cards.Add(controller);
        }
    }

    private void OnGameInitialized() {
        if (!IsInitialized) return;
        UpdateUI();
    }

    private void OnCreatureDied(ICreature creature) {
        if (!IsInitialized) return;

        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            if (card != null) {
                Destroy(card.gameObject);
            }
            creatureCards.Remove(creature.TargetId);
            cards.Remove(card);
            container?.UpdateUI();
        }
    }

    protected override void OnDestroy() {
        foreach (var card in creatureCards.Values) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        creatureCards.Clear();
        base.OnDestroy();
    }
}