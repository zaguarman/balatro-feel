using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldUI : MonoBehaviour {
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private RectTransform player1Battlefield;
    private RectTransform player2Battlefield;
    private Dictionary<string, Button> creatureButtons = new Dictionary<string, Button>();
    private IGameMediator gameMediator;

    private void Start() {
        InitializeReferences();
        RegisterEvents();
        UpdateBattlefield();
    }

    private void OnDestroy() {
        UnregisterEvents();
    }

    private void InitializeReferences() {
        var references = GameReferences.Instance;
        player1Battlefield = references.GetPlayer1Battlefield();
        player2Battlefield = references.GetPlayer2Battlefield();
        gameMediator = GameMediator.Instance;
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateBattlefield);
            gameMediator.OnCreatureDied.AddListener(OnCreatureDied);
        }
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateBattlefield);
            gameMediator.OnCreatureDied.RemoveListener(OnCreatureDied);
        }
    }

    private void UpdateBattlefield() {
        ClearBattlefield(player1Battlefield);
        ClearBattlefield(player2Battlefield);

        var gameManager = GameManager.Instance;
        CreateCreatureCards(gameManager.Player1, player1Battlefield, true);
        CreateCreatureCards(gameManager.Player2, player2Battlefield, false);
    }

    private void CreateCreatureCards(IPlayer player, RectTransform battlefield, bool isPlayer1) {
        if (battlefield == null || player == null) return;

        float totalWidth = cardOffset + (cardSpacing * player.Battlefield.Count);
        battlefield.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Battlefield.Count; i++) {
            CreateCreatureCard(player.Battlefield[i], battlefield, i, isPlayer1);
        }
    }

    private void CreateCreatureCard(ICreature creature, RectTransform battlefield, int index, bool isPlayer1) {
        var references = GameReferences.Instance;
        Button button = Instantiate(references.GetCardButtonPrefab(), battlefield);

        SetupCardTransform(button.GetComponent<RectTransform>(), index);
        SetupCardController(button.GetComponent<CardButtonController>(), creature, isPlayer1);

        creatureButtons[creature.TargetId] = button;
    }

    private void SetupCardTransform(RectTransform rect, int index) {
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);
    }

    private void SetupCardController(CardButtonController controller, ICreature creature, bool isPlayer1) {
        var creatureData = ScriptableObject.CreateInstance<CreatureData>();
        creatureData.cardName = creature.Name;
        creatureData.attack = creature.Attack;
        creatureData.health = creature.Health;

        controller.Setup(creatureData, isPlayer1);
    }

    private void ClearBattlefield(RectTransform battlefield) {
        if (battlefield == null) return;

        foreach (Transform child in battlefield) {
            Destroy(child.gameObject);
        }
        creatureButtons.Clear();
    }

    private void OnCreatureDied(ICreature creature) {
        if (creatureButtons.TryGetValue(creature.TargetId, out Button button)) {
            Destroy(button.gameObject);
            creatureButtons.Remove(creature.TargetId);
        }
        UpdateBattlefield();
    }
}