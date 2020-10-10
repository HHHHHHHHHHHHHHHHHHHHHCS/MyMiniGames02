using System;
using UnityEngine;
using UnityEngine.Playables;

namespace RoyaleBattle
{
    public class Building : ThinkingPlaceable
    {
        [Header("Timelines")] public PlayableDirector constructionTimeline;
        public PlayableDirector destructionTimeline;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Activate(Faction pFaction, PlaceableData pData)
        {
            pType = pData.pType;
            faction = pFaction;
            hitPoints = pData.hitPoints;
            targetType = pData.targetType;
            attackAudioClip = pData.attackClip;
            dieAudioClip = pData.dieClip;
            //TODO:添加更多的属性联系
            
            constructionTimeline.Play();
        }

        protected override void Die()
        {
            base.Die();
            //audioSource.PlayOneShot(dieAudioClip);
            
            destructionTimeline.Play();
        }
    }
}
