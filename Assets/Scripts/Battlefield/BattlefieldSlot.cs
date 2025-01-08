using UnityEngine;
using UnityEngine.UI;

public class BattlefieldSlot : MonoBehaviour, ITarget {
    private RectTransform rectTransform;
    private Image backgroundImage;

    public string TargetId { get; private set; }
    public CardController OccupyingCard { get; private set; }
    public ICreature OccupyingCreature { get; private set; }

    private Color defaultColor;
    private Color validDropColor;
    private Color invalidDropColor;
    private Color hoverColor;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        backgroundImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();

        TargetId = System.Guid.NewGuid().ToString();
    }

    public void Initialize(Color defaultColor, Color validDropColor, Color invalidDropColor, Color hoverColor) {
        this.defaultColor = defaultColor;
        this.validDropColor = validDropColor;
        this.invalidDropColor = invalidDropColor;
        this.hoverColor = hoverColor;

        backgroundImage.color = defaultColor;
        ClearSlot();
    }

    public void SetPosition(Vector2 position) {
        if (rectTransform != null) {
            rectTransform.anchoredPosition = position;
        }
    }

    public void AssignCreature(CardController card) {
        ClearSlot(); 
        if (card == null) return;

        OccupyingCard = card;
        OccupyingCreature = card.GetLinkedCreature(); 

        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
        card.UpdateUI();
    }

    public void OccupySlot(CardController card) {
        AssignCreature(card);
    }

    public void ClearSlot() {
        if (OccupyingCard != null) {
            Destroy(OccupyingCard.gameObject);
            OccupyingCard = null;
        }
        OccupyingCreature = null;
    }

    public bool IsOccupied() {
        return OccupyingCreature != null;
    }

    public void ResetVisuals() {
        if (backgroundImage != null) {
            backgroundImage.color = defaultColor;
        }
    }

    public bool IsValidTarget() => true;
}
