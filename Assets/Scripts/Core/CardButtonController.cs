using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButtonController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;

    [Header("Style")]
    [SerializeField] private Color player1Color = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2Color = new Color(1f, 0.8f, 0.8f);

    private Button button;
    private CardData cardData;
    private bool isPlayer1;

    private void Awake()
    {
        button = GetComponent<Button>();
        SetSize();
    }

    private void SetSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
    }

    public void Setup(CardData data, bool isPlayerOne)
    {
        cardData = data;
        isPlayer1 = isPlayerOne;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (cardData == null) return;

        // Update name
        if (nameText != null)
        {
            nameText.text = cardData.cardName;
        }

        // Update stats for creatures
        if (statsText != null)
        {
            if (cardData is CreatureData creatureData)
            {
                statsText.gameObject.SetActive(true);
                statsText.text = $"{creatureData.attack} / {creatureData.health}";
            }
            else
            {
                statsText.gameObject.SetActive(false);
            }
        }

        // Update description
        if (descriptionText != null)
        {
            descriptionText.text = cardData.description;
        }

        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isPlayer1 ? player1Color : player2Color;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    // Optional: Add hover effects
    public void OnPointerEnter()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit()
    {
        transform.localScale = Vector3.one;
    }
}
