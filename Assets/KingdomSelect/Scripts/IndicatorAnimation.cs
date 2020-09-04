using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomSelect
{
    public class IndicatorAnimation : MonoBehaviour
    {
        public RectTransform rect;
        private Image img;
        private Vector2 origSize;
        [Space] public float duration;
        public float delay;

        private void Start()
        {
            img = rect.GetComponent<Image>();
            img.DOFade(0, 0);

            origSize = rect.sizeDelta;
            rect.sizeDelta = origSize / 4f;
            
            StartCoroutine(Delay());
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(delay);
            Animate();
        }

        public void Animate()
        {
            Sequence s = DOTween.Sequence();
            //记录初始数值 缩放到原的数值
            s.Append(rect.DOSizeDelta(origSize, duration)).SetEase(Ease.OutCirc);
            s.Join(img.DOFade(1, duration / 3));
            s.Join(img.DOFade(0, duration / 4).SetDelay(duration / 1.5f));
            s.SetLoops(-1);//-1表示无限播放
        }
    }
}
