using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattlefieldUI : MonoBehaviour {
    private RectTransform player1Battlefield;
    private RectTransform player2Battlefield;
    private Button cardButtonPrefab;
    private Dictionary<string, Button> creatureButtons = new Dictionary<string, Button>();
    private ICreature selectedCreature;
    private bool isAttackMode;

    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    public void Start() {
        InitializeReferences();
        GameManager.Instance.OnGameStateChanged += UpdateBattlefield;
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

    public void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateBattlefield;
        }
    }
}