using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class HorizontalCardHolder : MonoBehaviour {

    [SerializeField] private BalatroCard selectedCard;
    [SerializeReference] private BalatroCard hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<BalatroCard> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    public void Start() {
        for (int i = 0; i < cardsToSpawn; i++) {
            Instantiate(slotPrefab, transform);
        }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<BalatroCard>().ToList();

        int cardCount = 0;

        foreach (BalatroCard card in cards) {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame() {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++) {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    private void BeginDrag(BalatroCard card) {
        selectedCard = card;
    }


    void EndDrag(BalatroCard card) {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(BalatroCard card) {
        hoveredCard = card;
    }

    void CardPointerExit(BalatroCard card) {
        hoveredCard = null;
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Delete)) {
            if (hoveredCard != null) {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1)) {
            foreach (BalatroCard card in cards) {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++) {

            if (selectedCard.transform.position.x > cards[i].transform.position.x) {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex()) {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x) {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex()) {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index) {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (BalatroCard card in cards) {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }

}
