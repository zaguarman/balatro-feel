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
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null) {
            backgroundImage = gameObject.AddComponent<Image>();
        }

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

    public void AssignCreature(ICreature creature) {
        OccupyingCreature = creature;
        // Occupy slot with creature card
    }

    public void OccupySlot(CardController card) {
        OccupyingCard = card;
        if (card != null) {
            card.transform.SetParent(transform);
            card.transform.localPosition = Vector3.zero;
        }
    }

    public void ClearSlot() {
        if (OccupyingCard != null) {
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