using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomHeartsShotLock.Scripts
{
	public class TargetDetection : MonoBehaviour
	{
		[Space, Header("Targets")] public List<Transform> targets = new List<Transform>();
		
		private Collider collider;

		private void Awake()
		{
			collider = GetComponent<Collider>();
		}

		public void SetCollider(bool state)
		{
			collider.enabled = state;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Enemy"))
			{
				var target = other.transform;
				if (!targets.Contains(target))
				{
					targets.Add(target);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Enemy"))
			{
				var target = other.transform;
				if (targets.Contains(target))
				{
					targets.Remove(target);
				}
			}
		}
	}
}