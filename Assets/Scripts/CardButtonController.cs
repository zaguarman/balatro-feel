using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButtonController : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Style")]
    [SerializeField] private Color player1Color = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2Color = new Color(1f, 0.8f, 0.8f);

    private Button button;
    private CardData cardData;
    private bool isPlayer1;
    private GameReferences gameRefs;
    private RectTransform cardRect;
    private Image buttonImage;

    public void Awake() {
        button = GetComponent<Button>();
        cardRect = GetComponent<RectTransform>();
        gameRefs = GameReferences.Instance;
        InitializeCard();
    }

    private void InitializeCard() {
        if (gameRefs == null) {
            Debug.LogError("GameReferences not found");
            return;
        }

        buttonImage = GetComponent<Image>();

        ValidateComponents();
        SetDefaultSize();
    }

    private void ValidateComponents() {
        if (nameText == null) Debug.LogError("Name Text component missing");
        if (statsText == null) Debug.LogError("Stats Text component missing");
        if (descriptionText == null) Debug.LogError("Description Text component missing");
        if (button == null) Debug.LogError("Button component missing");
        if (buttonImage == null) Debug.LogError("Image component missing");
    }

    private void SetDefaultSize() {
        if (cardRect != null) {
            // Set size based on parent container or default values
            Button prefab = gameRefs.GetCardButtonPrefab();
            if (prefab != null) {
                RectTransform prefabRect = prefab.GetComponent<RectTransform>();
                cardRect.sizeDelta = prefabRect.sizeDelta;
            }
        }
    }

    public void Setup(CardData data, bool isPlayerOne) {
        cardData = data;
        isPlayer1 = isPlayerOne;
        UpdateVisuals();

        // Set parent based on player
        Transform parent = isPlayer1 ?
            gameRefs.GetPlayer1CardContainer() :
            gameRefs.GetPlayer2CardContainer();

        if (parent != null) {
            transform.SetParent(parent, false);
        }
    }

    private void UpdateVisuals() {
        if (cardData == null) return;

        if (nameText != null) {
            nameText.text = cardData.cardName;
        }

        if (statsText != null) {
            if (cardData is CreatureData creatureData) {
                statsText.gameObject.SetActive(true);
                statsText.text = $"{creatureData.attack} / {creatureData.health}";
            } else {
                statsText.gameObject.SetActive(false);
            }
        }

        if (descriptionText != null) {
            descriptionText.text = cardData.description;
        }

        buttonImage.color = isPlayer1 ? player1Color : player2Color;
    }

    public void SetInteractable(bool interactable) {
        if (button != null) {
            button.interactable = interactable;
        }
    }

    public void OnPointerEnter() {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit() {
        transform.localScale = Vector3.one;
    }

    public void MoveToBattlefield() {
        if (gameRefs == null) return;

        Transform battlefield = isPlayer1 ?
            gameRefs.GetPlayer1Battlefield() :
            gameRefs.GetPlayer2Battlefield();

        if (battlefield != null) {
            transform.SetParent(battlefield, false);
        }
    }
}