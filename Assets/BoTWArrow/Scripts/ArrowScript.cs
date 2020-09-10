using System;
using DG.Tweening;
using UnityEngine;

namespace BoTWArrow.Scripts
{
	public class ArrowScript : MonoBehaviour
	{
		public GameObject hitParticle;


		private void Awake()
		{
			Destroy(gameObject, 5f);
		}

		private void OnCollisionEnter(Collision other)
		{
			var hitPart =  Instantiate(hitParticle, transform.position, Quaternion.identity);
			Destroy(hitPart,5f);


			Camera mainCam = Camera.main;
			mainCam.transform.DOComplete();
			mainCam.transform.DOShakePosition(0.4f, 0.5f, 20, 90, false, true);
			
			 Destroy(gameObject);
		}
	}
}