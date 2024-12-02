using System.Collections.Generic;
using UnityEngine;

public class BattlefieldUI : UIComponent {
    private CardContainer battlefield;
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();
    private IPlayer player;
    public void Initialize(CardContainer battlefield, IPlayer player) {
        if (battlefield == null) {
            Debug.LogError("Battlefield container is null in BattlefieldUI.Initialize");
            return;
        }

        this.battlefield = battlefield;
        this.player = player;

        if (battlefield != null) {
            InitializeContainer();
        }

        IsInitialized = true;
        UpdateUI();
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
        if (!IsInitialized || player == null || battlefield == null) return;
        UpdateBattlefieldCards();
    }

    private void InitializeContainer() {
        if (battlefield == null) {
            Debug.LogError("Battlefield container is null in BattlefieldUI.Initialize");
            return;
        }

        // Match the spacing used in CardContainer
        var settings = new ContainerSettings {
            layoutType = ContainerLayout.Horizontal,
            spacing = 220f,        // This is the same spacing used in HandUI
            offset = 50f,         // Same offset as HandUI
            cardMoveDuration = 0.15f,
            cardMoveEase = DG.Tweening.Ease.OutBack,
            cardHoverOffset = 30f
        };

        battlefield.SetSettings(settings);
        battlefield.SetPlayer(player);

        // Ensure RectTransform exists
        var rectTransform = battlefield.GetComponent<RectTransform>();
        if (rectTransform == null) {
            rectTransform = battlefield.gameObject.AddComponent<RectTransform>();
        }

        // Calculate the total width needed for 3 cards using the same formula as CardContainer
        float totalWidth = settings.offset + (settings.spacing * 3);
        rectTransform.sizeDelta = new Vector2(totalWidth, rectTransform.sizeDelta.y);

        // Set up the debug drop zone
        var dropZone = battlefield.gameObject.GetComponent<DebugDropZone>();
        if (dropZone == null) {
            dropZone = battlefield.gameObject.AddComponent<DebugDropZone>();
        }
    }

    private void UpdateBattlefieldCards() {
        foreach (var existingCard in creatureCards.Values) {
            if (existingCard != null) {
                Destroy(existingCard.gameObject);
            }
        }
        creatureCards.Clear();

        foreach (var creature in player.Battlefield) {
            CreateCreatureCard(creature);
        }

        battlefield.UpdateUI();
    }

    private void CreateCreatureCard(ICreature creature) {
        var references = GameReferences.Instance;
        var cardPrefab = references.GetCardPrefab();

        if (cardPrefab == null) return;

        var cardObj = Instantiate(cardPrefab, battlefield.transform);
        var controller = cardObj.GetComponent<CardController>();

        if (controller != null) {
            var creatureData = ScriptableObject.CreateInstance<CreatureData>();
            creatureData.cardName = creature.Name;
            creatureData.attack = creature.Attack;
            creatureData.health = creature.Health;

            controller.Setup(creatureData, player);
            creatureCards[creature.TargetId] = controller;
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
            battlefield?.UpdateUI();
        }
    }

    private void CleanupCards() {
        foreach (var card in creatureCards.Values) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        creatureCards.Clear();
    }
}