using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace RoyaleBattle
{
	public class CardManager : MonoBehaviour
	{
		public Camera mainCamera; //public reference
		public LayerMask playingFieldMask;
		public GameObject cardPrefab;
		public DeckData playersDeck;
		public MeshRenderer forbiddenAreaRenderer;

		public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;

		[Header("UI Elements")] public RectTransform backupCardTransform; //the smaller card that sits in the deck
		public RectTransform cardsDashboard; //the UI panel that contains the actual playable cards

		public RectTransform
			cardsPanel; //the UI panel that contains all cards, the deck, and the dashboard (center aligned)

		private Card[] cards;
		private bool cardIsActive = false; //when true, a card is being dragged over the play field
		private GameObject previewHolder;

		private Vector3
			inputCreationOffset =
				new Vector3(0f, 0f, 1f); //offsets the creation of units so that they are not under the player's finger

		private void Awake()
		{
			previewHolder = new GameObject("PreviewHolder");
			cards = new Card[3]; //3 is the length of the dashboard
		}

		public void LoadDeck()
		{
			DeckLoader newDeckLoaderComp = gameObject.AddComponent<DeckLoader>();
			newDeckLoaderComp.OnDeckLoaded += DeckLoaded;
			newDeckLoaderComp.LoadDeck(playersDeck);
		}

		private void DeckLoaded()
		{
			Debug.Log("Player's deck loaded");

			StartCoroutine(AddCardToDeck(0.1f));
		}

		//TODO:通过CardData动态数值
		private IEnumerator AddCardToDeck(float delay = 0f)
		{
			yield return new WaitForSeconds(delay);

			backupCardTransform = GameObject.Instantiate(cardPrefab, cardsPanel).GetComponent<RectTransform>();
			backupCardTransform.localScale = Vector3.one * 0.7f;

			backupCardTransform.anchoredPosition = new Vector2(180f, -300f);
			backupCardTransform.DOAnchorPos(new Vector2(180f, 0.0f), 0.2f).SetEase(Ease.OutQuad);

			Card cardScript = backupCardTransform.GetComponent<Card>();
			cardScript.InitialiseWithData(playersDeck.GetNextCardFromDeck());
		}
	}
}