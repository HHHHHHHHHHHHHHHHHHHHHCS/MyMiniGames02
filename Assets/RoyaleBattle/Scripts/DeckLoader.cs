using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoyaleBattle
{
	public class DeckLoader : MonoBehaviour
	{
		public UnityAction OnDeckLoaded;

		private DeckData targetDeck;

		public void LoadDeck(DeckData deckToLoad)
		{
			targetDeck = deckToLoad;
			Addressables.LoadAssetsAsync<CardData>(targetDeck.labelsToInclude[0].labelString, null).Completed +=
				OnResourcesRetrieved;
		}

		private void OnResourcesRetrieved(AsyncOperationHandle<IList<CardData>> obj)
		{
			targetDeck.CardsRetrieved((List<CardData>) obj.Result);
			OnDeckLoaded?.Invoke();
			Destroy(this);
		}
	}
}