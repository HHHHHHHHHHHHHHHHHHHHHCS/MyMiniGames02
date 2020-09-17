using System;
using DG.Tweening;
using UnityEngine;

namespace PersonaAllOut
{
	public class FinalScreen : MonoBehaviour
	{
		public CanvasGroup canvas;
		public Transform cam;
		public Transform canvasParent;

		[Space] [Header("Parameters")] public float fadeSpeed = 0.2f;

		private void Start()
		{
			canvas.DOFade(1, fadeSpeed);
			canvasParent.DOShakePosition(fadeSpeed, 300, 30, 90, false, true);
			cam.DOShakePosition(fadeSpeed * 2f, 0.5f, 40, 90, false, true);
		}
	}
}