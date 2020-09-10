using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BoTWArrow
{
	public class BowScript : MonoBehaviour
	{
		[Header("Bow")] public Transform bowModel;
		public Transform bowZoomTransform;
		private Vector3 bowOriginalPos, bowOriginalRot;

		[Space] [Header("Arrow")] public GameObject arrowPrefab;
		public Transform arrowSpawnOrigin;
		public Transform arrowModel;
		private Vector3 arrowOriginalPos;

		[Space] [Header("Parameters")] public Vector3 arrowImpulse;
		public float timeToShoot;
		public float shootWait;
		public bool canShoot;
		public bool shootRest;
		public bool isAiming;

		[Space] public float zoomInDuration;
		public float zoomOutDuration;


		private float camOriginFov;
		public float camZoomFov;
		private Vector3 camOriginalPos;
		public Vector3 camZoomOffset;

		[Space] [Header("Particles")] public ParticleSystem[] prepareParticles;
		public ParticleSystem[] aimParticles;
		public GameObject circleParticlePrefab;

		[Space] [Header("Canvas")] public RectTransform reticle;
		public CanvasGroup reticleCanvas;
		public Image centerCircle;
		private Vector2 originalImageSize;

		private Camera mainCamera;

		private void Start()
		{
			mainCamera = Camera.main;
			camOriginalPos = mainCamera.transform.localPosition;
			camOriginFov = mainCamera.fieldOfView;
			bowOriginalPos = bowModel.transform.localPosition;
			bowOriginalRot = bowModel.transform.localEulerAngles;
			arrowOriginalPos = arrowModel.transform.localPosition;

			originalImageSize = reticle.sizeDelta;
			ShowReticle(false, 0);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (!shootRest && !isAiming)
				{
					canShoot = false;
					isAiming = true;

					StopCoroutine(PrepareSequence());
					StartCoroutine(PrepareSequence());

					ShowReticle(true, zoomInDuration / 2);

					Transform bow = bowZoomTransform;

					arrowModel.transform.localPosition = arrowOriginalPos;
					arrowModel.DOLocalMoveZ(arrowModel.transform.localPosition.z - 0.1f, zoomInDuration * 2f);
					CameraZoom(camZoomFov, camOriginalPos + camZoomOffset, bow.localPosition, bow.localEulerAngles,
						zoomInDuration, true);
				}
			}

			if (Input.GetKeyUp(KeyCode.Mouse0))
			{
				if (!shootRest && isAiming)
				{
					StopCoroutine(ShootSequence());
					StartCoroutine(ShootSequence());
				}
			}
		}

		private void CameraZoom(float fov, Vector3 camPos, Vector3 bowPos, Vector3 bowRot, float duration, bool zoom)
		{
			mainCamera.transform.DOComplete();
			mainCamera.DOFieldOfView(fov, duration);
			mainCamera.transform.DOLocalMove(camPos, duration);
			bowModel.transform.DOLocalRotate(bowRot, duration).SetEase(Ease.OutBack);
			bowModel.transform.DOLocalMove(bowPos, duration).OnComplete(() => ShowArrow(zoom));
		}

		private void ShowArrow(bool state)
		{
			bowModel.GetChild(0).gameObject.SetActive(state);
		}

		private IEnumerator PrepareSequence()
		{
			foreach (var particle in prepareParticles)
			{
				particle.Play();
			}

			yield return new WaitForSeconds(timeToShoot);

			canShoot = true;

			foreach (var particle in aimParticles)
			{
				particle.Play();
			}
		}

		private IEnumerator ShootSequence()
		{
			yield return new WaitUntil(() => canShoot == true);

			shootRest = true;

			isAiming = false;
			canShoot = false;

			ShowReticle(false, zoomOutDuration);

			CameraZoom(camOriginFov, camOriginalPos, bowOriginalPos, bowOriginalRot, zoomOutDuration, true);
			arrowModel.transform.localPosition = arrowOriginalPos;

			var go = Instantiate(circleParticlePrefab, arrowSpawnOrigin.position, bowModel.rotation);
			Destroy(go,5f);
			
			GameObject arrow = Instantiate(arrowPrefab, arrowSpawnOrigin.position, bowModel.rotation);
			arrow.GetComponent<Rigidbody>().AddForce(transform.forward * arrowImpulse.z + transform.up * arrowImpulse.y,
				ForceMode.Impulse);
			ShowArrow(false);

			yield return new WaitForSeconds(shootWait);
			shootRest = false;
		}

		private void ShowReticle(bool state, float duration)
		{
			float num = state ? 1 : 0;
			reticleCanvas.DOFade(num, duration);
			Vector2 size = state ? originalImageSize / 2 : originalImageSize;
			reticle.DOComplete();
			reticle.DOSizeDelta(size, duration * 4);

			if (state)
			{
				centerCircle.DOFade(1, 0.5f).SetDelay(duration * 3);
			}
			else
			{
				centerCircle.DOFade(0, duration);
			}
		}
	}
}