using UnityEngine;
using System.Collections.Generic;

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

    protected override void UpdateContainerSettings() {
        base.UpdateContainerSettings();

        // Configure the drop zone
        var dropZone = container.gameObject.GetComponent<DebugDropZone>();
        if (dropZone == null) {
            dropZone = container.gameObject.AddComponent<DebugDropZone>();
        }
    }

    public override void UpdateUI() {
        if (!IsInitialized || player == null || container == null) return;
        UpdateBattlefieldCards();
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

        // Create new cards
        foreach (var creature in player.Battlefield) {
            CreateCreatureCard(creature);
        }

        // Update layout
        UpdateLayout(player.Battlefield.Count);
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
            UpdateLayout(player.Battlefield.Count);
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