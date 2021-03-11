using System;
using DG.Tweening;
using UnityEngine;

namespace OverwatchDragonstrike.Scripts
{
	public class DragonMovement : MonoBehaviour
	{
		private static readonly int TimeOffset_ID = Shader.PropertyToID("_TimeOffset");
		private static readonly int SplitValue_ID = Shader.PropertyToID("_SplitValue");

		public float speed = 5f;

		[Space, Header("Shader Settings")] public float initialDissolveValue = -75f;
		public float finalDissolveValue = 75f;
		public float dissolveSpeed = 2f;
		public float firstDragonOffset = 1f;

		[Space] public float destroyTime = 10f;

		private Renderer[] renderers;
		private float splitValue;

		private void Start()
		{
			renderers = GetComponentsInChildren<Renderer>();

			foreach (var item in renderers[0].materials)
			{
				item.SetFloat(TimeOffset_ID, firstDragonOffset);
			}

			splitValue = initialDissolveValue;

			foreach (var item in renderers)
			{
				var materials = item.materials;

				foreach (var mat in materials)
				{
					mat.SetFloat(SplitValue_ID, splitValue);
					mat.DOFloat(initialDissolveValue, SplitValue_ID, 1)
						.SetDelay(destroyTime).SetUpdate(UpdateType.Late); //.OnComplete(() => Destroy(gameObject));
				}
			}

			Destroy(gameObject, destroyTime + 1);
		}

		private void Update()
		{
			transform.localPosition += transform.forward * Time.deltaTime * speed;

			splitValue += dissolveSpeed * Time.deltaTime * speed;

			foreach (var item in renderers)
			{
				var materials = item.materials;

				foreach (var mat in materials)
				{
					//其实可以传入一个起始时间  然后Shader中 _Time.x - startTime
					mat.SetFloat(SplitValue_ID, splitValue);
				}
			}
		}
	}
}