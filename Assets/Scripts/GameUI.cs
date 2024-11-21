using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour {
    [Header("Player UI")]
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private RectTransform player1CardContainer;
    [SerializeField] private RectTransform player2CardContainer;

    [Header("Resolution UI")]
    [SerializeField] private DamageResolver damageResolver;

    [Header("Card Layout")]
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;
    [SerializeField] private List<CardData> testCards;

    public void OnEnable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged += UpdateUI;
        }
        if (GameMediator.Instance != null) {
            GameMediator.Instance.RegisterUI(this);
        }
    }

    public void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateUI;
        }
        if (GameMediator.Instance != null) {
            GameMediator.Instance.UnregisterUI(this);
        }
    }

    public void Start() {
        SetupUI();
        UpdateUI();
    }

    private void SetupCardContainer(RectTransform container) {
        var existingLayout = container.GetComponent<LayoutGroup>();
        if (existingLayout != null) {
            Destroy(existingLayout);
        }

        float totalWidth = cardOffset + (cardSpacing * testCards.Count);
        container.sizeDelta = new Vector2(totalWidth, 320f);
    }

    private void CreateCardButton(CardData cardData, RectTransform parent, bool isPlayer1, int index) {
        Button buttonObj = Instantiate(cardButtonPrefab, parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        float xPos = cardOffset + (cardSpacing * index);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(xPos, 0);

        CardButtonController controller = buttonObj.GetComponent<CardButtonController>();
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(
                cardData,
                isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2
            );
        });
    }

    public void CreateCardButtons() {
        SetupCardContainer(player1CardContainer);
        SetupCardContainer(player2CardContainer);

        for (int i = 0; i < testCards.Count; i++) {
            CreateCardButton(testCards[i], player1CardContainer, true, i);
            CreateCardButton(testCards[i], player2CardContainer, false, i);
        }
    }

    private void SetupUI() {
        LoadTestCards();
        CreateCardButtons();

        if (damageResolver != null) {
            damageResolver.gameObject.SetActive(true);
        }
    }

    private void LoadTestCards() {
        if (testCards == null || testCards.Count == 0) {
            var testSetup = GetComponent<TestSetup>();
            if (testSetup == null) {
                testSetup = gameObject.AddComponent<TestSetup>();
            }
            testCards = testSetup.CreateTestCards();
        }
    }

    public void UpdateUI() {
        UpdatePlayerHealth(GameManager.Instance.Player1);
        UpdatePlayerHealth(GameManager.Instance.Player2);
        UpdateResolutionUI();
    }

    public void UpdatePlayerHealth(IPlayer player) {
        TextMeshProUGUI healthText = player == GameManager.Instance.Player1 ?
            player1HealthText : player2HealthText;

        if (healthText != null) {
            healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateResolutionUI() {
        if (damageResolver != null) {
            damageResolver.UpdateResolutionState();
        }
    }

    private void HandlePlayerDamaged(IPlayer player) {
        UpdatePlayerHealth(player);
    }



    public void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameStateChanged -= UpdateUI;
            GameManager.Instance.OnPlayerDamaged -= HandlePlayerDamaged;
        }
    }
}