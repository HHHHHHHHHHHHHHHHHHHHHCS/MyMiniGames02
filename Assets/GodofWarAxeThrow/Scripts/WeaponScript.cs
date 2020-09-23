using System;
using GodofWarAxeThrow.Scripts;
using UnityEngine;

namespace GodofWarAxeThrow
{
	public class WeaponScript : MonoBehaviour
	{
		public bool activated;

		public float rotationSpeed;

		public LayerMask groundLayer;


		private void Update()
		{
			if (activated)
			{
				transform.localEulerAngles += Vector3.forward * rotationSpeed * Time.deltaTime;
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			var layer = (int) Mathf.Log(groundLayer.value, 2f);
			if (other.gameObject.layer == layer)
			{
				Debug.Log(other.gameObject.name);
				var rigi = GetComponent<Rigidbody>();
				rigi.Sleep();
				rigi.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
				rigi.isKinematic = true;
				activated = false;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Breakable"))
			{
				var boxScript = other.GetComponent<BreakBoxScript>();
				if (boxScript != null)
				{
					boxScript.Break();
				}
			}
		}
	}
}