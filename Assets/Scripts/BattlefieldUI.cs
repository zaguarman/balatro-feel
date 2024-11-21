using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattlefieldUI : MonoBehaviour {
    [Header("Battlefield Layout")]
    [SerializeField] private RectTransform player1Battlefield;
    [SerializeField] private RectTransform player2Battlefield;
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private Dictionary<string, Button> creatureButtons = new Dictionary<string, Button>();
    private ICreature selectedCreature;
    private bool isAttackMode;

    public void Start() {
        GameManager.Instance.OnGameStateChanged += UpdateBattlefield;
        UpdateBattlefield();
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

            button.onClick.AddListener(() => HandleCreatureClick(creature, player));

            creatureButtons[creature.TargetId] = button;
        }
    }

    private void HandleCreatureClick(ICreature creature, IPlayer owner) {
        if (selectedCreature == null) {
            if (owner == GameManager.Instance.Player1) {
                selectedCreature = creature;
                isAttackMode = true;
                HighlightValidTargets();
            }
        } else if (isAttackMode) {
            if (owner == GameManager.Instance.Player2) {
                GameManager.Instance.AttackWithCreature(selectedCreature, GameManager.Instance.Player1, creature);
            }
            ResetAttackMode();
        }
    }

    private void HighlightValidTargets() {
        foreach (var button in creatureButtons) {
            CardButtonController controller = button.Value.GetComponent<CardButtonController>();
            bool isValidTarget = GameManager.Instance.Player2.Battlefield.Exists(c => c.TargetId == button.Key);
            controller.SetInteractable(isValidTarget);
        }
    }

    private void ResetAttackMode() {
        selectedCreature = null;
        isAttackMode = false;
        foreach (var button in creatureButtons) {
            button.Value.GetComponent<CardButtonController>().SetInteractable(true);
        }
    }

    public void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateBattlefield;
        }
    }
}