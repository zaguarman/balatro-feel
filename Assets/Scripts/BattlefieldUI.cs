using System.Collections.Generic;
using UnityEngine;

public class BattlefieldUI : UIComponent {
    private CardContainer battlefield;
    private Dictionary<string, CardController> creatureCards = new Dictionary<string, CardController>();
    private GameMediator gameMediator;
    private IPlayer player;
    private bool isInitialized;

    private void InitializeReferences() {
        gameMediator = GameMediator.Instance;
        RegisterEvents();
    }

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

        isInitialized = true;
        UpdateUI();
    }

    private void InitializeContainer() {
        if (battlefield == null) {
            Debug.LogError("Battlefield is null in BattlefieldUI.InitializeContainer");
            return;
        }

        var settings = new ContainerSettings {
            layoutType = ContainerLayout.Horizontal,
            spacing = 220f,
            offset = 50f,
            cardMoveDuration = 0.15f,
            cardMoveEase = DG.Tweening.Ease.OutBack,
            cardHoverOffset = 30f
        };

        battlefield.SetSettings(settings);

        // Ensure RectTransform exists
        var rectTransform = battlefield.GetComponent<RectTransform>();
        if (rectTransform == null) {
            rectTransform = battlefield.gameObject.AddComponent<RectTransform>();
        }

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

    public override void UpdateUI() {
        if (!isInitialized || player == null || battlefield == null) return;

        UpdateBattlefieldCards();
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
        if (!isInitialized) return;
        UpdateUI();
    }

    private void OnCreatureDied(ICreature creature) {
        if (!isInitialized) return;

        if (creatureCards.TryGetValue(creature.TargetId, out CardController card)) {
            if (card != null) {
                Destroy(card.gameObject);
            }
            creatureCards.Remove(creature.TargetId);
            battlefield?.UpdateUI();
        }
    }

    private void OnDestroy() {
        InitializationManager.Instance.OnSystemInitialized.RemoveListener(InitializeReferences);
        UnregisterEvents();
        CleanupCards();
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