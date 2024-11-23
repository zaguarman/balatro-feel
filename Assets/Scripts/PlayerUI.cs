using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerUIReferences {
    public TextMeshProUGUI healthText;
    public RectTransform handContainer;
    public RectTransform battlefieldContainer;
}

// PlayerUI.cs
public class PlayerUI : MonoBehaviour {
    private IPlayer player;
    private PlayerUIReferences references;
    private bool isPlayer1;
    private IGameMediator gameMediator;

    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private List<CardButtonController> handCards = new List<CardButtonController>();

    private void Start() {
        gameMediator = GameMediator.Instance;
        RegisterEvents();
    }

    private void OnDestroy() {
        UnregisterEvents();
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnPlayerDamaged.AddListener(HandlePlayerDamaged);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        }
    }

    public void SetReferences(PlayerUIReferences refs) {
        references = refs;
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        player = isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
    }

    public void UpdateUI() {
        UpdateHealth();
        UpdateHand();
    }

    private void UpdateHealth() {
        if (player != null && references.healthText != null) {
            references.healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateHand() {
        ClearContainer(references.handContainer);
        handCards.Clear();

        if (player == null || references.handContainer == null) return;

        float totalWidth = cardOffset + (cardSpacing * player.Hand.Count);
        references.handContainer.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void CreateCardInHand(ICard card, int index) {
        var buttonObj = CreateCardButton(references.handContainer, index);
        var controller = buttonObj.GetComponent<CardButtonController>();

        var cardData = CreateCardData(card);
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(cardData, player);
        });

        handCards.Add(controller);
    }

    private Button CreateCardButton(RectTransform parent, int index) {
        Button buttonObj = Instantiate(GameReferences.Instance.GetCardButtonPrefab(), parent);
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

    private void ClearContainer(RectTransform container) {
        if (container == null) return;
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }
    }

    private CardData CreateCardData(ICard card) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = card.Name;

        if (card is ICreature creature) {
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        }

        return cardData;
    }
}
