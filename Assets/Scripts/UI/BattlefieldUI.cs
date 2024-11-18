using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class BattlefieldUI : MonoBehaviour 
{
    [Header("Battlefield Layout")]
    [SerializeField] private RectTransform player1Battlefield;
    [SerializeField] private RectTransform player2Battlefield;
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private Dictionary<string, Button> creatureButtons = new Dictionary<string, Button>();
    private Creature selectedCreature;
    private bool isAttackMode;

    private void Start() 
    {
        GameManager.Instance.OnGameStateChanged += UpdateBattlefield;
        UpdateBattlefield();
    }

    public void UpdateBattlefield() 
    {
        ClearBattlefield(player1Battlefield);
        ClearBattlefield(player2Battlefield);
        
        CreateCreatureCards(GameManager.Instance.Player1, player1Battlefield, true);
        CreateCreatureCards(GameManager.Instance.Player2, player2Battlefield, false);
    }

    private void ClearBattlefield(RectTransform battlefield) 
    {
        foreach (Transform child in battlefield) {
            Destroy(child.gameObject);
        }
    }

    private void CreateCreatureCards(Player player, RectTransform battlefield, bool isPlayer1) 
    {
        float totalWidth = cardOffset + (cardSpacing * player.Battlefield.Count);
        battlefield.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Battlefield.Count; i++) {
            Creature creature = player.Battlefield[i];
            Button button = Instantiate(cardButtonPrefab, battlefield);
            
            // Position
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * i), 0);

            // Setup
            CardButtonController controller = button.GetComponent<CardButtonController>();
            CreatureData creatureData = ScriptableObject.CreateInstance<CreatureData>();
            creatureData.cardName = creature.Name;
            creatureData.attack = creature.Attack;
            creatureData.health = creature.Health;
            controller.Setup(creatureData, isPlayer1);

            // Attack logic
            button.onClick.AddListener(() => HandleCreatureClick(creature, player));

            creatureButtons[creature.Id] = button;
        }
    }

    private void HandleCreatureClick(Creature creature, Player owner) 
    {
        if (selectedCreature == null) {
            // Select creature for attacking
            if (owner == GameManager.Instance.Player1) {
                selectedCreature = creature;
                isAttackMode = true;
                HighlightValidTargets();
            }
        } 
        else if (isAttackMode) {
            // Attack target
            if (owner == GameManager.Instance.Player2) {
                GameManager.Instance.AttackWithCreature(selectedCreature, GameManager.Instance.Player1, creature);
            }
            ResetAttackMode();
        }
    }

    private void HighlightValidTargets() 
    {
        foreach (var button in creatureButtons) {
            CardButtonController controller = button.Value.GetComponent<CardButtonController>();
            bool isValidTarget = GameManager.Instance.Player2.Battlefield.Exists(c => c.Id == button.Key);
            controller.SetInteractable(isValidTarget);
        }
    }

    private void ResetAttackMode() 
    {
        selectedCreature = null;
        isAttackMode = false;
        foreach (var button in creatureButtons) {
            button.Value.GetComponent<CardButtonController>().SetInteractable(true);
        }
    }

    private void OnDestroy() 
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateBattlefield;
        }
    }
}
