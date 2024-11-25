using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldUI : UIComponent {
    private CardContainer player1Battlefield;
    private CardContainer player2Battlefield;
    private Dictionary<string, CardButtonController> creatureCards = new Dictionary<string, CardButtonController>();
    private GameMediator gameMediator;

    private void Start() {
        InitializeReferences();
        InitializeContainers();
    }

    private void InitializeReferences() {
        var references = GameReferences.Instance;
        player1Battlefield = references.GetPlayer1Battlefield();
        player2Battlefield = references.GetPlayer2Battlefield();
        gameMediator = GameMediator.Instance;
    }

    private void InitializeContainers() {
        // Configure Player 1's battlefield
        if (player1Battlefield != null) {
            var settings1 = new ContainerSettings {
                layoutType = ContainerLayout.Horizontal,
                spacing = 220f,
                offset = 50f,
                cardMoveDuration = 0.15f,
                cardMoveEase = DG.Tweening.Ease.OutBack,
                cardHoverOffset = 30f
            };

            var dropZone1 = player1Battlefield.gameObject.AddComponent<BattlefieldDropZone>();
            dropZone1.acceptPlayer1Cards = true;
            dropZone1.acceptPlayer2Cards = false;
        }

        // Configure Player 2's battlefield
        if (player2Battlefield != null) {
            var settings2 = new ContainerSettings {
                layoutType = ContainerLayout.Horizontal,
                spacing = 220f,
                offset = 50f,
                cardMoveDuration = 0.15f,
                cardMoveEase = DG.Tweening.Ease.OutBack,
                cardHoverOffset = 30f
            };

            var dropZone2 = player2Battlefield.gameObject.AddComponent<BattlefieldDropZone>();
            dropZone2.acceptPlayer1Cards = false;
            dropZone2.acceptPlayer2Cards = true;
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnCreatureDied.AddListener(OnCreatureDied);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnCreatureDied.RemoveListener(OnCreatureDied);
        }
    }

    public override void UpdateUI() {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        UpdatePlayerBattlefield(gameManager.Player1, player1Battlefield, true);
        UpdatePlayerBattlefield(gameManager.Player2, player2Battlefield, false);
    }

    private void UpdatePlayerBattlefield(IPlayer player, CardContainer battlefield, bool isPlayer1) {
        if (battlefield == null || player == null) return;

        // Clear existing cards but maintain the dictionary
        foreach (var existingCard in creatureCards.Values) {
            if (existingCard != null) {
                Destroy(existingCard.gameObject);
            }
        }
        creatureCards.Clear();

        // Create new cards for each creature
        foreach (var creature in player.Battlefield) {
            CreateCreatureCard(creature, battlefield, isPlayer1);
        }

        // Let the container handle layout
        battlefield.UpdateUI();
    }

    private void CreateCreatureCard(ICreature creature, CardContainer battlefield, bool isPlayer1) {
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

            controller.Setup(creatureData, isPlayer1);

            // Store reference in dictionary
            creatureCards[creature.TargetId] = controller;
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (creatureCards.TryGetValue(creature.TargetId, out CardButtonController card)) {
            if (card != null) {
                var container = card.transform.parent.GetComponent<CardContainer>();
                if (container != null) {
                    container.RemoveCard(card);
                }
            }
            creatureCards.Remove(creature.TargetId);
        }

        // Update both battlefields
        player1Battlefield?.UpdateUI();
        player2Battlefield?.UpdateUI();
    }

    private void OnDestroy() {
        // Clean up any remaining cards
        foreach (var card in creatureCards.Values) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        creatureCards.Clear();
    }
}