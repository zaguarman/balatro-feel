using UnityEngine;
using System.Collections.Generic;

public class BattlefieldUI : UIComponent {
    private CardContainer battlefield;
    private Dictionary<string, CardButtonController> creatureCards = new Dictionary<string, CardButtonController>();
    private GameMediator gameMediator;
    private IPlayer player;
    private bool isInitialized = false;

    private void Start() {
        InitializeReferences();
    }

    private void InitializeReferences() {
        gameMediator = GameMediator.Instance;
        RegisterEvents();
    }

    public void Initialize(CardContainer battlefield, IPlayer player) {
        this.battlefield = battlefield;
        this.player = player;

        if (battlefield != null) {
            InitializeContainer();
        }

        isInitialized = true;
        UpdateUI();
    }

    private void InitializeContainer() {
        if (battlefield == null) return;

        var settings = new ContainerSettings {
            layoutType = ContainerLayout.Horizontal,
            spacing = 220f,
            offset = 50f,
            cardMoveDuration = 0.15f,
            cardMoveEase = DG.Tweening.Ease.OutBack,
            cardHoverOffset = 30f
        };

        battlefield.SetSettings(settings);

        var dropZone = battlefield.gameObject.AddComponent<BattlefieldDropZone>();
        dropZone.acceptPlayer1Cards = player == GameManager.Instance.Player1;
        dropZone.acceptPlayer2Cards = player == GameManager.Instance.Player2;
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

    private void OnGameInitialized() {
        if (!isInitialized) return;
        UpdateUI();
    }

    public override void UpdateUI() {
        if (!isInitialized || player == null || battlefield == null) return;

        // Clear existing cards but maintain the dictionary
        foreach (var existingCard in creatureCards.Values) {
            if (existingCard != null) {
                Destroy(existingCard.gameObject);
            }
        }
        creatureCards.Clear();

        // Create new cards for each creature
        foreach (var creature in player.Battlefield) {
            CreateCreatureCard(creature);
        }

        // Let the container handle layout
        battlefield.UpdateUI();
    }

    private void CreateCreatureCard(ICreature creature) {
        var references = GameReferences.Instance;
        var cardPrefab = references.GetCardButtonPrefab();

        if (cardPrefab == null) return;

        var cardObj = Instantiate(cardPrefab, battlefield.transform);
        var controller = cardObj.GetComponent<CardButtonController>();

        if (controller != null) {
            // Setup the card data
            var creatureData = ScriptableObject.CreateInstance<CreatureData>();
            creatureData.cardName = creature.Name;
            creatureData.attack = creature.Attack;
            creatureData.health = creature.Health;

            controller.Setup(creatureData, player);

            // Store reference in dictionary
            creatureCards[creature.TargetId] = controller;
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (!isInitialized) return;

        if (creatureCards.TryGetValue(creature.TargetId, out CardButtonController card)) {
            if (card != null) {
                var container = card.transform.parent.GetComponent<CardContainer>();
                if (container != null) {
                    container.RemoveCard(card);
                }
            }
            creatureCards.Remove(creature.TargetId);
        }

        battlefield?.UpdateUI();
    }

    private void OnDestroy() {
        UnregisterEvents();

        // Clean up any remaining cards
        foreach (var card in creatureCards.Values) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        creatureCards.Clear();
    }
}