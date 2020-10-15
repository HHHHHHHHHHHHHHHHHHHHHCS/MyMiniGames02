using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoyaleBattle
{
	public class Card : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
	{
		public UnityAction<int, Vector2> OnDragAction;
		public UnityAction<int> OnTapDownAction, OnTapReleaseAction;

		[HideInInspector] public int cardId;
		[HideInInspector] public CardData cardData;

		public Image portraitImage; //Inspector-set reference
		private CanvasGroup canvasGroup;

		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		public void InitialiseWithData(CardData cData)
		{
			cardData = cData;
			portraitImage.sprite = cardData.cardImage;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnTapDownAction?.Invoke(cardId);
		}

		public void OnDrag(PointerEventData eventData)
		{
			OnDragAction?.Invoke(cardId, eventData.delta);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			OnTapReleaseAction?.Invoke(cardId);
		}
		
		public void ChangeActiveState(bool isActive)
		{
			canvasGroup.alpha = (isActive) ? .05f : 1f;
		}
	}
}