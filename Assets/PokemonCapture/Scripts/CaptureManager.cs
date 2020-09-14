using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PokemonCapture.Scripts
{
	public class CaptureManager : MonoBehaviour
	{
		[Header("Public References")] public Transform pokeball;
		public Transform pokemon;
		[Space] [Header("Throw Settings")] public float throwArc;
		public float throwDuration;
		[Space] [Header("Hit Settings")] public Vector3 hitOffset;
		public Transform jumpPosition;
		public float jumpPower = .5f;
		public float jumpDuration;
		[Space] [Header("Open Settings")] public float openAngle;
		public float openDuration;
		[Space] [Header("Open Settings")] public float fallDuration = .6f;
		[Space] [Header("Cameras Settings")] public GameObject secondCamera;
		public float finalZoomDuration = .5f;

		[Space] [Header("Particles")] public ParticleSystemForceField forceField;
		public ParticleSystem throwParticle;
		public ParticleSystem firstLines;
		public ParticleSystem firstCircle;
		public ParticleSystem firstFlash;
		public ParticleSystem firstDust;
		public ParticleSystem beam;

		[Space] public ParticleSystem capture1;
		public ParticleSystem capture2;
		public ParticleSystem capture3;

		[Space] public ParticleSystem yellowBlink;
		public ParticleSystem blueBlink;
		public ParticleSystem finalCircle;
		public ParticleSystem stars;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ThrowPokeBall();
			}

			if (Input.GetKey(KeyCode.S))
			{
				Time.timeScale = 0.5f;
			}
			else
			{
				Time.timeScale = 1f;
			}

			ReLoad();
		}

		private void ThrowPokeBall()
		{
			Sequence throwSequence = DOTween.Sequence();

			//throw the pokeball
			throwSequence.Append(pokeball.DOJump(pokemon.position + hitOffset, throwArc, 1, throwDuration));
			throwSequence.Join(pokeball.DORotate(new Vector3(300, 0, 0), throwDuration, RotateMode.FastBeyond360));

			//PokeBall jump
			throwSequence.Append(pokeball.DOJump(jumpPosition.position, jumpPower, 1, jumpDuration));
			throwSequence.Join(pokeball.DOLookAt(pokemon.position, jumpDuration));

			throwSequence.AppendCallback(() =>
			{
				throwParticle.Stop();
				firstCircle.Play();
				firstLines.Play();
				firstFlash.Play();
				firstDust.Play();
				//Pokemon Disappear
				PokemonDisappear();
			});

			//Pokeball Open
			throwSequence
				.Append(pokeball.GetChild(0).GetChild(0).DOLocalRotate(new Vector3(-openAngle, 0, 0), openDuration)
					.SetEase(Ease.OutBack));
			throwSequence.Join(pokeball.GetChild(0).GetChild(1)
				.DOLocalRotate(new Vector3(openAngle, 0, 0), openDuration)
				.SetEase(Ease.OutBack));

			throwSequence.AppendCallback(() => forceField.gameObject.SetActive(true));
			throwSequence.Join(firstDust.transform.DORotate(new Vector3(0, 0, 100), 0.5f, RotateMode.FastBeyond360));
			throwSequence.Join(beam.transform.DOMove(jumpPosition.position, 0.2f));

			throwSequence.AppendCallback(ChangeCamera);

			//Pokeball Close
			throwSequence.Append(pokeball.GetChild(0).GetChild(0).DOLocalRotate(Vector3.zero, openDuration / 3));
			throwSequence.Join(pokeball.GetChild(0).GetChild(1).DOLocalRotate(Vector3.zero, openDuration / 3));

			throwSequence.AppendCallback(() =>
			{
				capture1.Play();
				capture2.Play();
				capture3.Play();
				secondCamera.transform.DOShakePosition(.2f, .1f, 15, 90, false, true);
			});
			throwSequence.Join(pokeball.DORotate(Vector3.zero, openDuration / 3).SetEase(Ease.OutBack));

			//Interval
			throwSequence.AppendInterval(.3f);

			//Pokeball Fall
			throwSequence.Append(pokeball.DOMoveY(.18f, fallDuration).SetEase(Ease.OutBounce));
			throwSequence.Join(pokeball.DOPunchRotation(new Vector3(-40, 0, 0), fallDuration, 5, 10));
		}

		private void PokemonDisappear()
		{
			pokemon.DOScale(0f, 0.3f);
		}

		private void ChangeCamera()
		{
			secondCamera.SetActive(true);
			
			firstCircle.Stop();
			firstLines.Stop();
			firstFlash.Stop();
			firstDust.Stop();

			Transform cam = secondCamera.transform;

			Sequence cameraSequence = DOTween.Sequence();
			cameraSequence.Append(cam.DOMoveY(0.3f, 1.5f)).SetDelay(0.5f);

			cameraSequence.AppendInterval(0.5f);
			cameraSequence.Append(cam.DOMoveZ(0.3f, finalZoomDuration).SetEase(Ease.InExpo));

			//Particle
			cameraSequence.AppendCallback(yellowBlink.Play);
			cameraSequence.Join(pokeball.GetChild(0).DOShakeRotation(0.5f, 30, 8, 70, true));

			cameraSequence.AppendInterval(0.8f);
			cameraSequence.Append(cam.DOMoveZ(0.0f, finalZoomDuration).SetEase(Ease.InExpo));

			//Particle
			cameraSequence.AppendCallback(yellowBlink.Play);
			cameraSequence.Join(pokeball.GetChild(0).DOShakeRotation(0.5f, 20, 8, 70, true));

			cameraSequence.AppendInterval(0.8f);
			cameraSequence.Append(cam.DOMoveZ(-0.2f, finalZoomDuration).SetEase(Ease.InExpo));

			//Particle
			cameraSequence.AppendCallback(() => yellowBlink.Play());
			cameraSequence.Join(pokeball.GetChild(0).DOShakeRotation(.5f, 10, 8, 70, true));

			cameraSequence.AppendInterval(.8f);


			//Particle
			cameraSequence.AppendCallback(() =>
			{
				blueBlink.Play();
				finalCircle.Play();
				stars.Play();

				secondCamera.transform.DOShakePosition(.2f, .1f, 7, 90, false, true);
			});

			cameraSequence.Append(pokeball.GetChild(0).DOPunchRotation(new Vector3(-10, 0, 0), .5f, 8, 1));
		}


		private void ReLoad()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}
		}

		private void OnDrawGizmos()
		{
			if (pokemon == null)
			{
				return;
			}

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(pokemon.position + hitOffset, 0.2f);
		}
	}
}