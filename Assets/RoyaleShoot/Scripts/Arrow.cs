using System;
using UnityEngine;

namespace RoyaleShoot
{
	public class Arrow : MonoBehaviour
	{
		public float forceIntensity = 50f;

		private Rigidbody rigi;

		public void Awake()
		{
			//arrow 的轴 不对
			rigi = transform.GetComponent<Rigidbody>();
			rigi.AddForce(-transform.up * forceIntensity,
				ForceMode.Impulse);
			GameObject.Destroy(gameObject, 7f);
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!other.transform.CompareTag("Player"))
			{
				rigi.Sleep();
				rigi.isKinematic = false;
			}
		}
	}
}