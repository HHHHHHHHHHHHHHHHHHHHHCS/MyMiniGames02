using System;
using System.Collections;
using System.Security.Cryptography;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KingdomHeartsShotLock.Scripts
{
	public class ProjectileScript : MonoBehaviour
	{
		public float rotationSpeed = 5f;
		public float movementSpeed = 10f;
		public float initialWait = 1f;
		public bool initial = true;

		public Transform target;
		public GameObject hitParticle;

		private float multiplier = 1f;

		private void Start()
		{
			multiplier = Random.Range(1, 3f);
			StartCoroutine(InitialWait());
			Destroy(gameObject, 3f);
		}

		private void Update()
		{
			if (initial)
			{
				transform.eulerAngles += Vector3.forward * Time.deltaTime * (rotationSpeed / 2f);
				transform.position += transform.up * Time.deltaTime * (movementSpeed / 8f);
			}
			else
			{
				var targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.5f);
				transform.position += transform.forward * Time.deltaTime * movementSpeed * multiplier;
				transform.GetChild(0).eulerAngles += Vector3.forward * Time.deltaTime * rotationSpeed * 1.5f;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.transform == target)
			{
				Instantiate(hitParticle, transform.position, quaternion.identity);
				var trail = transform.GetChild(0).GetChild(0);
				Destroy(trail.gameObject, 0.8f);
				trail.parent = null;
				Destroy(gameObject);
			}
		}

		private IEnumerator InitialWait()
		{
			yield return new WaitForSeconds(initialWait / 2f);
			transform.GetChild(0).GetChild(0).DOLocalMoveY(1f, 0.2f);
			yield return new WaitForSeconds(initialWait / 2f);
			DOVirtual.Float(rotationSpeed, rotationSpeed * 1.5f, 0.3f, SetRotationSpeed);
			initial = false;
		}

		private void SetRotationSpeed(float x)
		{
			rotationSpeed = x;
		}
	}
}