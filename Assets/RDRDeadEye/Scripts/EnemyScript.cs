using System;
using UnityEngine;

namespace RDRDeadEye
{
	public class EnemyScript : MonoBehaviour
	{
		[HideInInspector] public bool aimed;
		[HideInInspector] public Transform aimingPoint;

		private Rigidbody[] rigis;
		private Animator anim;
		private ShooterController shooter;

		private void Start()
		{
			shooter = ShooterController.instance;
			anim = GetComponent<Animator>();
			rigis = GetComponentsInChildren<Rigidbody>();
			Ragdoll(false);
		}

		public void Ragdoll(bool state)
		{
			anim.enabled = !state;

			foreach (var rigi in rigis)
			{
				rigi.isKinematic = !state;
			}

			if (state)
			{
				GetComponent<Rigidbody>()
					.AddForce((aimingPoint.transform.position - shooter.transform.position).normalized * 30, ForceMode.Impulse);
				Destroy(aimingPoint);
			}
		}

		public void CreateAimingPoint(Transform parent, Vector3 hitPoint)
		{
			if (aimingPoint == null)
			{
				aimingPoint = new GameObject("AimingPoint").transform;
			}

			aimingPoint.transform.SetParent(parent);
			aimingPoint.position = hitPoint;
		}
	}
}