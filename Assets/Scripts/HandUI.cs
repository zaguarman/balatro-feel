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

    // Dragging methods
    protected override void OnCardBeginDrag(CardController card) {
        if (card == null) return;
        card.transform.SetAsLastSibling();
    }

    protected override void OnCardEndDrag(CardController card) {
        UpdateLayout();
    }

    // We don't need OnCardDropped anymore as the receiving container (BattlefieldUI) 
    // handles the drop through its OnDrop method
    protected override void OnCardDropped(CardController card) {
        UpdateLayout();
    }

    // Hover methods (empty as before)
    protected override void OnCardHoverEnter(CardController card) {
        // Do nothing on hover
    }

    protected override void OnCardHoverExit(CardController card) {
        // Do nothing on hover exit
    }
}