using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PersonaAllOut
{
	public class MirrorBreak : MonoBehaviour
	{
		public float breakDuration;
		public Transform cam;
		public Transform mirrorParent;

		private void Start()
		{
			for (int i = 0; i < mirrorParent.childCount; i++)
			{
				Transform ts = mirrorParent.GetChild(i);
				ts.DOLocalRotate(new Vector3(Random.Range(0, 20), 0, Random.Range(0, 20)), breakDuration);
				ts.DOScale(mirrorParent.GetChild(i).localScale / 1.2f, breakDuration);
			}

			cam.DOShakePosition(breakDuration, 0.5f, 20, 90, false, true);
		}
	}
}