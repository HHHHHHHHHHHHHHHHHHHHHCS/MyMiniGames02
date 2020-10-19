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
		public MeshRenderer forbiddenAreaRenderer; //禁止放置的板子

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
			for (int i = 0; i < cards.Length; i++)
			{
				StartCoroutine(PromoteCardFromDeck(i, 0.4f + i));
				StartCoroutine(AddCardToDeck(0.8f + i));
			}
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

		private IEnumerator PromoteCardFromDeck(int position, float delay = 0f)
		{
			yield return new WaitForSeconds(delay);

			backupCardTransform.SetParent(cardsDashboard, true);
			backupCardTransform.DOAnchorPos(new Vector2(210f * (position + 1) + 20f, 0f),
				0.2f + (0.05f * position)).SetEase(Ease.OutQuad);
			backupCardTransform.localScale = Vector3.one;

			Card cardScript = backupCardTransform.GetComponent<Card>();
			cardScript.cardId = position;
			cards[position] = cardScript;

			cardScript.OnTapDownAction += CardTapped;
			cardScript.OnDragAction += CardDragged;
			cardScript.OnTapReleaseAction += CardReleased;
		}

		private void CardTapped(int cardId)
		{
			cards[cardId].GetComponent<RectTransform>().SetAsLastSibling();
			forbiddenAreaRenderer.enabled = true; //禁止放置的板子
		}


		private void CardDragged(int cardId, Vector2 dragAmount)
		{
			cards[cardId].transform.Translate(dragAmount);

			RaycastHit hit;
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

			bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);

			if (planeHit)
			{
				if (!cardIsActive)
				{
					cardIsActive = true;
					previewHolder.transform.position = hit.point;
					cards[cardId].ChangeActiveState(true);

					PlaceableData[] dataToSpawn = cards[cardId].cardData.placeablesData;
					Vector3[] offsets = cards[cardId].cardData.relativeOffsets;

					for (int i = 0; i < dataToSpawn.Length; i++)
					{
						GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn[i].associatedPrefab,
							hit.point + offsets[i] + inputCreationOffset, Quaternion.identity, previewHolder.transform);
					}
				}
				else
				{
					previewHolder.transform.position = hit.point;
				}
			}
			else
			{
				if (cardIsActive)
				{
					cardIsActive = false;
					cards[cardId].ChangeActiveState(false);

					ClearPreviewObjects();
				}
			}
		}

		private void CardReleased(int cardId)
		{
			forbiddenAreaRenderer.enabled = false;

			RaycastHit hit;
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
			{
				if (OnCardUsed != null)
				{
					OnCardUsed(cards[cardId].cardData, hit.point + inputCreationOffset, Placeable.Faction.Player);
				}

				ClearPreviewObjects();
				Destroy(cards[cardId].gameObject);

				StartCoroutine(PromoteCardFromDeck(cardId, 0.2f));
				StartCoroutine(AddCardToDeck(0.6f));
			}
			else
			{
				cards[cardId].GetComponent<RectTransform>().DOAnchorPos(new Vector2(210f * (cardId+1), 0f),
					.2f).SetEase(Ease.OutQuad);
			}
		}


		private void ClearPreviewObjects()
		{
			for (int i = 0; i < previewHolder.transform.childCount; i++)
			{
				Destroy(previewHolder.transform.GetChild(i).gameObject);
			}
		}
	}
}