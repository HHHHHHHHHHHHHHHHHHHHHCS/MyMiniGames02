using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomHeartsShotLock.Scripts.UI
{
	public class LockFollowUI : MonoBehaviour
	{
		private RectTransform rect;
		private CanvasGroup canvas;
		public Transform target;

		private void Start()
		{
			canvas = GetComponent<CanvasGroup>();
			rect = transform.GetChild(0).GetComponent<RectTransform>();
			Animate();
		}

		public void Animate()
		{
			canvas.DOComplete();
			rect.DOComplete();
			transform.DOComplete();

			canvas.alpha = 0;
			rect.sizeDelta = new Vector2(500, 500);
			transform.GetChild(0).localScale = Vector3.one * 10;

			rect.DOSizeDelta(new Vector2(100, 100), 0.2f).SetEase(Ease.InOutSine);
			transform.GetChild(0).DOScale(1f, 0.4f);
			canvas.DOFade(1f, 0.5f);
		}

		private void Update()
		{
			if (target != null)
			{
				transform.position = Camera.main.WorldToScreenPoint(target.position);
			}
		}
	}
}