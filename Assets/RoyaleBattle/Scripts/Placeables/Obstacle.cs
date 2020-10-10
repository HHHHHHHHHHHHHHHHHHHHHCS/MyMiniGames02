using System;
using System.Collections;
using UnityEngine;

namespace RoyaleBattle
{
    public class Obstacle : Placeable
    {
        [HideInInspector] public float timeToRemoval;

        private AudioSource audioSource;

        private void Awake()
        {
            pType = PlaceableType.Obstacle;
            faction = Faction.None;
            audioSource = GetComponent<AudioSource>();
        }

        public void Activate(PlaceableData pData)
        {
            timeToRemoval = pData.lifeTime;
            dieAudioClip = pData.dieClip;
            //TODO:添加更多的属性联系

            StartCoroutine(Die());
        }

        private IEnumerator Die()
        {
            yield return  new WaitForSeconds(timeToRemoval);

            if (OnDie != null)
            {
                OnDie(this);
            }
            
            Destroy(gameObject);
        }
    }
}
