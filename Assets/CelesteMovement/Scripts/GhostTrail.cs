using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace CelesteMovement.Scripts
{
    public class GhostTrail : MonoBehaviour
    {
        public PlayerMovement move;
        public PlayerAnimation anim;
        
        public Transform ghostsParent;
        public Color trailColor;
        public Color fadeColor;
        public float ghostInterval;
        public float fadeTime;

        private Sequence sequence;


        public void ShowGhost()
        {
            sequence?.Kill();

            foreach (Transform child in ghostsParent)
            {
                sequence.AppendCallback(() =>
                {
                    child.position = move.transform.position;
                    var spr = child.GetComponent<SpriteRenderer>();
                    spr.flipX = anim.sr.flipX;
                    spr.sprite = anim.sr.sprite;
                    spr.material.color = trailColor;
                    FadeSprite(spr);
                });
                sequence.AppendInterval(ghostInterval);
            }
        }
        
        public void FadeSprite(SpriteRenderer current)
        {
            current.material.DOKill();
            current.material.DOColor(fadeColor, fadeTime);
        }
    }
}
