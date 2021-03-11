using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OverwatchDragonstrike.Scripts
{
	public class DragonStrike : MonoBehaviour
	{
		private static readonly int GlowPower_ID = Shader.PropertyToID("_GlowPower");
		private static readonly int Amount_ID = Shader.PropertyToID("_Amount");


		public bool canUltimate = true;

		public Transform arrow;

		public float arrowForce = 3;
		public float arrowPullDistance = 0.2f;
		public float arrowPullDuration = 1.0f;

		public float dragonSummonWait = 1f;
		public GameObject dragonStrikePrefab;
		public GameObject portalPrefab;

		private Rigidbody arrowRigi;

		private Vector3 arrowLocalPos;
		private Quaternion arrowLocalRot;

		private WaitForSeconds pullWait;
		private WaitForSeconds dragWait;
		private WaitForSeconds ultimateWait;


		private void Awake()
		{
			arrowRigi = arrow.GetComponent<Rigidbody>();
			arrowLocalPos = arrow.localPosition;
			arrowLocalRot = arrow.localRotation;

			pullWait = new WaitForSeconds(arrowPullDuration);
			dragWait = new WaitForSeconds(dragonSummonWait);
			ultimateWait = new WaitForSeconds(8f);
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0) && canUltimate)
			{
				StartCoroutine(UltimateCourotine());
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}
		}

		private IEnumerator UltimateCourotine()
		{
			canUltimate = false;

			//pull arrow
			arrow.DOLocalMoveZ(arrow.localPosition.z - arrowPullDistance, arrowPullDuration).SetEase(Ease.OutExpo);
			var arrowRender = arrow.GetComponent<Renderer>();
			arrowRender.material.DOFloat(1, GlowPower_ID, arrowPullDuration);

			var mainCam = Camera.main;

			var oldFov = mainCam.fieldOfView;

			mainCam.DOFieldOfView(65, arrowPullDuration).SetEase(Ease.OutSine);

			yield return pullWait;

			ThrowArrow();

			mainCam.DOFieldOfView(oldFov, 0.5f).SetEase(Ease.OutBack);
			var q = Quaternion.LookRotation(transform.forward, Vector3.up);

			yield return dragWait;

			//Instantiate
			var dragon = Instantiate(dragonStrikePrefab, arrow.position, q);
			var portal = Instantiate(portalPrefab, arrow.position - (arrow.forward * 5) + (Vector3.up * 1.2f), q);

			//Show Portal
			portal.transform.DOScale(0, 0.2f).SetEase(Ease.OutSine).From();
			portal.GetComponent<Renderer>().material.DOFloat(1, Amount_ID, 4f).SetDelay(8)
				.OnComplete(() => Destroy(portal));

			ParticleSystem[] portalParticles = portal.GetComponentsInChildren<ParticleSystem>();
			foreach (var item in portalParticles)
			{
				item.Play();
			}

			//Extras
			arrow.GetComponent<TrailRenderer>().emitting = false; //拖尾轨迹
			arrowRigi.isKinematic = true;
			arrowRender.enabled = false;
			arrow.parent = transform.GetChild(0);
			arrow.transform.localPosition = arrowLocalPos;
			arrow.transform.localRotation = arrowLocalRot;
			arrowRender.enabled = true;
			arrowRender.material.SetFloat(GlowPower_ID, 1f);
			arrowRender.material.DOFloat(0.0f, GlowPower_ID, 0.5f);

			mainCam.transform.DOShakePosition(0.2f, 0.5f, 20, 90, false, true);

			yield return ultimateWait;
			canUltimate = true;
		}

		private void ThrowArrow()
		{
			arrow.parent = null;
			arrowRigi.isKinematic = false;
			arrowRigi.AddForce(-arrow.forward * arrowForce, ForceMode.Impulse);
			arrow.GetComponent<TrailRenderer>().enabled = true;

			arrow.GetComponent<Renderer>().material.SetFloat(GlowPower_ID, 0f);

			ParticleSystem[] portalParticles = arrow.GetComponentsInChildren<ParticleSystem>();
			foreach (var item in portalParticles)
			{
				item.Play();
			}
		}
	}
}