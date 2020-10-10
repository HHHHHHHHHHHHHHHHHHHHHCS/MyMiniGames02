using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace RoyaleBattle
{
    public class CPUOpponent : MonoBehaviour
    {
        public DeckData aiDeck;
        public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;

        public float opponentLoopTime = 5f;
        
        private bool act;
        private Coroutine actingCoroutine;

        public void LoadDeck()
        {
            DeckLoader newDeckLoaderCom = gameObject.AddComponent<DeckLoader>();
            newDeckLoaderCom.OnDeckLoaded += DeckLoaded;
            newDeckLoaderCom.LoadDeck(aiDeck);
        }
        
        private void DeckLoaded()
        {
            Debug.Log("AI deck loaded");

            //StartActing();
        }

        public void StartActing()
        {
            Invoke("Bridge",0f);
        }

        private void Bridge()
        {
            act = true;
            actingCoroutine = StartCoroutine(CreateRandomCards());
        }

        public void StopActing()
        {
            act = false;
            StopCoroutine(actingCoroutine);
        }

        //TODO:create a proper AI
        private IEnumerator CreateRandomCards()
        {
            while (act)
            {
                yield return  new WaitForSeconds(opponentLoopTime);

                if (OnCardUsed != null)
                {
                    Vector3 newPos = new Vector3(Random.Range(-5f,5f),0f,Random.Range(3f,8.5f));
                    OnCardUsed(aiDeck.GetNextCardFromDeck(), newPos, Placeable.Faction.Opponent);
                }
            }
        }
    }
}
