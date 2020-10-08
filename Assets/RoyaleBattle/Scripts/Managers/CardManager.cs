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
        
		[Header("UI Elements")]
		public RectTransform backupCardTransform; //the smaller card that sits in the deck
		public RectTransform cardsDashboard; //the UI panel that contains the actual playable cards
		public RectTransform cardsPanel; //the UI panel that contains all cards, the deck, and the dashboard (center aligned)
        
		private Card[] cards;
		private bool cardIsActive = false; //when true, a card is being dragged over the play field
		private GameObject previewHolder;
		private Vector3 inputCreationOffset = new Vector3(0f, 0f, 1f); //offsets the creation of units so that they are not under the player's finger

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
	}
}
