using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattlefieldUI : MonoBehaviour {
    private RectTransform player1Battlefield;
    private RectTransform player2Battlefield;
    private Button cardButtonPrefab;
    private Dictionary<string, Button> creatureButtons = new Dictionary<string, Button>();
    private IGameMediator gameMediator;

    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    public void Start() {
        InitializeReferences();
        // Get GameMediator instance and subscribe to events
        gameMediator = GameMediator.Instance;
        gameMediator.OnGameStateChanged.AddListener(UpdateBattlefield);
        gameMediator.OnCreatureDied.AddListener(OnCreatureDied);
        UpdateBattlefield();
    }

    private void InitializeReferences() {
        player1Battlefield = GameReferences.Instance.GetPlayer1Battlefield();
        player2Battlefield = GameReferences.Instance.GetPlayer2Battlefield();
        cardButtonPrefab = GameReferences.Instance.GetCardButtonPrefab();
    }

    public void UpdateBattlefield() {
        ClearBattlefield(player1Battlefield);
        ClearBattlefield(player2Battlefield);
        CreateCreatureCards(GameManager.Instance.Player1, player1Battlefield, true);
        CreateCreatureCards(GameManager.Instance.Player2, player2Battlefield, false);
    }

    private void ClearBattlefield(RectTransform battlefield) {
        foreach (Transform child in battlefield) {
            Destroy(child.gameObject);
        }
        creatureButtons.Clear();
    }

    private void CreateCreatureCards(IPlayer player, RectTransform battlefield, bool isPlayer1) {
        float totalWidth = cardOffset + (cardSpacing * player.Battlefield.Count);
        battlefield.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Battlefield.Count; i++) {
            ICreature creature = player.Battlefield[i];
            Button button = Instantiate(cardButtonPrefab, battlefield);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * i), 0);

            CardButtonController controller = button.GetComponent<CardButtonController>();
            CreatureData creatureData = ScriptableObject.CreateInstance<CreatureData>();
            creatureData.cardName = creature.Name;
            creatureData.attack = creature.Attack;
            creatureData.health = creature.Health;
            controller.Setup(creatureData, isPlayer1);

            creatureButtons[creature.TargetId] = button;
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (creatureButtons.TryGetValue(creature.TargetId, out Button button)) {
            Destroy(button.gameObject);
            creatureButtons.Remove(creature.TargetId);
        }
        UpdateBattlefield();
    }

    public void OnDestroy() {
        if (gameMediator != null) {
            // Unsubscribe from all events
            gameMediator.OnGameStateChanged.RemoveListener(UpdateBattlefield);
            gameMediator.OnCreatureDied.RemoveListener(OnCreatureDied);
        }
    }
}