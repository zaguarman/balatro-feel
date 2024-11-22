using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerUIReferences {
    public TextMeshProUGUI healthText;
    public RectTransform handContainer;
    public RectTransform battlefieldContainer;
}

// Manages UI for a single player
public class PlayerUI : UIComponent {
    [SerializeField] private PlayerUIReferences references;
    [SerializeField] private bool isPlayer1;
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private IPlayer player;
    private List<CardButtonController> handCards = new List<CardButtonController>();
    private List<CardButtonController> battlefieldCards = new List<CardButtonController>();

    protected override void RegisterEvents() {
        Events.OnGameStateChanged.AddListener(UpdateUI);
        Events.OnPlayerDamaged.AddListener(HandlePlayerDamaged);
        Events.OnCreatureDied.AddListener(HandleCreatureDied);
    }

    protected override void UnregisterEvents() {
        Events.OnGameStateChanged.RemoveListener(UpdateUI);
        Events.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        Events.OnCreatureDied.RemoveListener(HandleCreatureDied);
    }

    protected void Start() {
        player = isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
        ValidateReferences();
    }

    private void ValidateReferences() {
        if (references.healthText == null) Debug.LogError($"Health text missing for {(isPlayer1 ? "Player 1" : "Player 2")}");
        if (references.handContainer == null) Debug.LogError($"Hand container missing for {(isPlayer1 ? "Player 1" : "Player 2")}");
        if (references.battlefieldContainer == null) Debug.LogError($"Battlefield container missing for {(isPlayer1 ? "Player 1" : "Player 2")}");
    }

    public override void UpdateUI() {
        UpdateHealth();
        UpdateHand();
        UpdateBattlefield();
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
    }

    private void UpdateHealth() {
        if (player != null && references.healthText != null) {
            references.healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateHand() {
        ClearContainer(references.handContainer);
        handCards.Clear();

        float totalWidth = cardOffset + (cardSpacing * player.Hand.Count);
        references.handContainer.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void UpdateBattlefield() {
        ClearContainer(references.battlefieldContainer);
        battlefieldCards.Clear();

        float totalWidth = cardOffset + (cardSpacing * player.Battlefield.Count);
        references.battlefieldContainer.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Battlefield.Count; i++) {
            CreateCardInBattlefield(player.Battlefield[i], i);
        }
    }

    private void CreateCardInHand(ICard card, int index) {
        var buttonObj = CreateCardButton(references.handContainer, index);
        var controller = buttonObj.GetComponent<CardButtonController>();

        // Convert ICard to CardData
        var cardData = CreateCardData(card);
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(cardData, player);
        });

        handCards.Add(controller);
    }

    private void CreateCardInBattlefield(ICreature creature, int index) {
        var buttonObj = CreateCardButton(references.battlefieldContainer, index);
        var controller = buttonObj.GetComponent<CardButtonController>();

        var creatureData = ScriptableObject.CreateInstance<CreatureData>();
        creatureData.cardName = creature.Name;
        creatureData.attack = creature.Attack;
        creatureData.health = creature.Health;

        controller.Setup(creatureData, isPlayer1);
        battlefieldCards.Add(controller);
    }

    private Button CreateCardButton(RectTransform parent, int index) {
        Button buttonObj = Instantiate(References.GetCardButtonPrefab(), parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);

        return buttonObj;
    }

    private void HandlePlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            UpdateHealth();
        }
    }

    private void HandleCreatureDied(ICreature creature) {
        if (player.Battlefield.Contains(creature)) {
            UpdateBattlefield();
        }
    }

    private void ClearContainer(RectTransform container) {
        if (container == null) return;
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }
    }

    private CardData CreateCardData(ICard card) {
        // Implementation would depend on your card system
        // This is a simplified example
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = card.Name;

        if (card is ICreature creature) {
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        }

        return cardData;
    }
}