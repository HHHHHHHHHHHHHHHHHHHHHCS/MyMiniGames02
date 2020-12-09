using System;
using UnityEngine;

namespace RDRDeadEye
{
	public class EnemyScript : MonoBehaviour
	{
		public bool aimed;

		private Rigidbody[] rigis;
		private Animator anim;
		private ShooterController shooter;

		private void Start()
		{
			shooter = ShooterController.instance;
			anim = GetComponent<Animator>();
			rigis = GetComponentsInChildren<Rigidbody>();
			Ragdoll(false, null);
		}

		public void Ragdoll(bool state, Transform point)
		{
			anim.enabled = !state;
			
			foreach (var rigi in rigis)
			{
				rigi.isKinematic = !state;
			}

			if (state)
			{
				point.GetComponent<Rigidbody>()
					.AddForce((point.position - shooter.transform.position).normalized * 30, ForceMode.Impulse);
			}
		}
	}
}