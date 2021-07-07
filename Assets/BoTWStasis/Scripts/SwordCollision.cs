using System;
using UnityEngine;

namespace BoTWStasis.Scripts
{
	public class SwordCollision : MonoBehaviour
	{
		[Header("Settings")] public float collisionForce = 15;
		public LayerMask layerMask;
		public Vector3 hitPoint;

		[Space] [Header("Particle")] public GameObject hitParticle;
		public GameObject stasisHitParticle;

		private BoxCollider boxCollider;

		private void Awake()
		{
			boxCollider = GetComponent<BoxCollider>();
		}

		private void OnTriggerEnter(Collider other)
		{
			RaycastHit hit;

			if (Physics.Raycast(transform.position + transform.forward, -transform.forward, out hit, 8, layerMask))
			{
				var so = hit.transform.GetComponent<StasisObject>();
				if (so == null)
				{
					return;
				}

				hitPoint = hit.point;
				so.AccumulateForce(collisionForce, hit.point);

				Instantiate(hitParticle, hit.point, Quaternion.identity);

				if (!so.activated)
				{
					return;
				}

				GameObject stasisHit = Instantiate(stasisHitParticle, hit.point, Quaternion.identity);
				var stasisParticles = stasisHit.GetComponentsInChildren<ParticleSystem>();

				foreach (var particle in stasisParticles)
				{
					var pMain = particle.main;
					pMain.startColor = so.particleColor;
				}
			}
		}

		private void OnDrawGizmos()
		{
			if (boxCollider == null || !boxCollider.enabled)
			{
				return;
			}

			Gizmos.color = Color.red;
			Ray ray = new Ray(transform.position + transform.forward, -transform.forward);
			Gizmos.DrawRay(ray);

			Gizmos.DrawSphere(hitPoint, 0.2f);
		}
	}
}