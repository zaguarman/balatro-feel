using System.Linq;
using UnityEngine;

public class HandUI : BaseCardContainer {
    private void Start() {
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            InitializeReferences();
        } else {
            InitializationManager.Instance.OnSystemInitialized.AddListener(InitializeReferences);
        }
    }

    private void InitializeReferences() {
        if (!InitializationManager.Instance.IsComponentInitialized<GameManager>()) {
            Debug.LogWarning("GameManager not initialized yet, will retry later");
            return;
        }

        if (player != null) {
            Initialize(player);
        }
    }

    public override void Initialize(IPlayer assignedPlayer) {
        player = assignedPlayer;
        base.Initialize(player);
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddGameInitializedListener(OnGameInitialized);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveGameInitializedListener(OnGameInitialized);
        }
    }

    private void OnGameInitialized() {
        UpdateUI();
    }

    public override void UpdateUI() {
        if (player == null) return;

        // Clear existing cards
        foreach (var card in cards.ToList()) {
            if (card != null) {
                CleanupCardEventHandlers(card);
                Destroy(card.gameObject);
            }
        }
        cards.Clear();

        // Create new cards for each card in hand
        foreach (var cardData in player.Hand) {
            var controller = CreateCard(cardData);
            if (controller != null) {
                AddCard(controller);
            }
        }

        UpdateLayout();
    }

    protected override void OnCardDropped(CardController card) {
        var gameManager = GameManager.Instance;
        if (gameManager != null && CardDropZone.IsOverDropZone(card.transform.position, out ICardDropZone targetDropZone)) {
            gameManager.PlayCard(card.GetCardData(), player);
            gameMediator?.NotifyGameStateChanged();
        }
    }

    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;
        card.transform.SetAsLastSibling();
    }

    protected override void OnCardEndDrag(CardController card) {
        UpdateLayout();
    }

    // Override hover methods to do nothing
    protected override void OnCardHoverEnter(CardController card) {
        // Do nothing on hover
    }

    protected override void OnCardHoverExit(CardController card) {
        // Do nothing on hover exit
    }
}