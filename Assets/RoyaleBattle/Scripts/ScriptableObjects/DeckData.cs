using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoyaleBattle
{
	[CreateAssetMenu(fileName = "NewDeck", menuName = "Unity Royale/Deck Data")]
	public class DeckData : ScriptableObject
	{
		public AssetLabelReference[] labelsToInclude;

		private CardData[] cards;
		private int currentCard = 0;

		public void CardsRetrieved(List<CardData> cardDataDownloaded)
		{
			int totalCards = cardDataDownloaded.Count;
			cards = new CardData[totalCards];
			for (int c = 0; c < totalCards; c++)
			{
				cards[c] = cardDataDownloaded[c];
			}
		}

		public void ShuffleCards()
		{
			//TODO: shuffle cards
		}

		public CardData GetNextCardFromDeck()
		{
			currentCard++;
			if (currentCard >= cards.Length)
			{
				currentCard = 0;
			}

			return cards[currentCard];
		}
	}
}