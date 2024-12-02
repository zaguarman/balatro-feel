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
            container = player.IsPlayer1()
                ? gameReferences.GetPlayer1Hand()
                : gameReferences.GetPlayer2Hand();

            if (container != null) {
                Initialize(container, player);
            }
        }
    }

    public void Initialize(IPlayer player) {
        this.player = player;
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            InitializeReferences();
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
        }
    }

    public override void UpdateUI() {
        if (player == null || container == null) return;

        ClearCards();
        UpdateContainerSize(player.Hand.Count);
        CreateHandCards();
    }

    private void CreateHandCards() {
        if (player.Hand == null) return;

        for (int i = 0; i < player.Hand.Count; i++) {
            var controller = CreateCard(player.Hand[i], container.transform);
            if (controller != null) {
                SetupCardPosition(controller.GetComponent<RectTransform>(), i);
                cards.Add(controller);
            }
        }
    }

    private void SetupCardPosition(RectTransform rect, int index) {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);
    }

    protected override void OnCardDropped(CardController card) {
        var gameManager = GameManager.Instance;
        if (gameManager != null && CardDropZone.IsOverDropZone(card.transform.position, out ICardDropZone targetDropZone)) {
            gameManager.PlayCard(card.GetCardData(), player);
            gameMediator?.NotifyGameStateChanged();
        }
    }
}