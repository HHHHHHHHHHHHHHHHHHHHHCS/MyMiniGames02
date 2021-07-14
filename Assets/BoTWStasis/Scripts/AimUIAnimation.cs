using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BoTWStasis.Scripts
{
	public class AimUIAnimation : MonoBehaviour
	{
		public static AimUIAnimation instance;

		[SerializeField] private RectTransform animationSquare;
		[SerializeField] private Sprite noTargetSprite;
		[SerializeField] private Sprite targetSprite;

		private CanvasGroup canvasGroup;


		private void Awake()
		{
			instance = this;
			canvasGroup = GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0;
			//LoopType.Yoyo == pingpong
			animationSquare.DOSizeDelta(animationSquare.sizeDelta / 1.4f, 0.4f)
				.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
		}

		public void Show(bool state)
		{
			float alpha = state ? 1 : 0;
			canvasGroup.DOFade(alpha, 0.2f);
		}

		public void Target(bool state)
		{
			Sprite s = state ? targetSprite : noTargetSprite;
			for (int i = 0; i < animationSquare.transform.childCount; i++)
			{
				animationSquare.transform.GetChild(i).GetComponent<Image>().sprite = s;
			}
		}
	}
}