using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RDRDeadEye
{
	public class ShooterController : MonoBehaviour
	{
		private static readonly int aiming_ID = Animator.StringToHash("aiming");

		public static ShooterController instance { get; private set; }

		private MovementInput input;
		private Animator anim;

		[Header("Cinemachine")] public CinemachineFreeLook thirdPersonCam;
		private CinemachineImpulseSource impulse;


		public Volume postVolume;
		private VolumeProfile postProfile;
		private ColorAdjustments colorAdjustments;
		private ChromaticAberration chromaticAberration;
		private Vignette vignette;

		public Color deadEyeColor;
		private Color currentColor = Color.white;


		[Space, Header("Booleans")] public bool aiming; //处于瞄准模式
		public bool deadEye; //处于射击模式

		[Space, Header("Settings")] private float originalZoom;
		public float originalOffsetAmount;
		public float zoomOffsetAmount;
		public float aimTime;
		[Header("Targets")] public List<Transform> targets = new List<Transform>();

		[Space, Header("UI")] public GameObject aimPrefab;
		public List<Transform> crossList = new List<Transform>();
		public Transform canvas;
		public Image reticle;

		[Space, Header("Gun")] public Transform gun;
		public Vector3 gunAimPos = new Vector3(0.3273799f, -0.03389892f, -0.08808608f);
		public Vector3 gunAimRot = new Vector3(-1.763f, -266.143f, -263.152f);
		private Vector3 gunIdlePos;
		private Vector3 gunIdleRot;

		private void Awake()
		{
			instance = this;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			anim = GetComponent<Animator>();
			input = GetComponent<MovementInput>();
			originalZoom = thirdPersonCam.m_Orbits[1].m_Radius;
			impulse = thirdPersonCam.GetComponent<CinemachineImpulseSource>();

			postProfile = postVolume.profile;
			postProfile.TryGet(out colorAdjustments);
			postProfile.TryGet(out chromaticAberration);
			postProfile.TryGet(out vignette);

			gunIdlePos = gun.localPosition;
			gunIdleRot = gun.localEulerAngles;

			HorizontalOffset(originalOffsetAmount);
		}

		private void Update()
		{
			ReLoad();

			if (aiming)
			{
				if (targets.Count > 0)
				{
					for (int i = 0; i < targets.Count; i++)
					{
						crossList[i].position = Camera.main.WorldToScreenPoint(targets[i].position);
					}
				}
			}

			//处于射击模式 不处理
			if (deadEye)
			{
				return;
			}

			anim.SetFloat("speed", input.speed);

			if (!aiming)
			{
				WeaponPosition();
			}

			if (Input.GetMouseButtonDown(1) && !deadEye)
			{
				Aim(true);
			}

			if (Input.GetMouseButtonUp(1) && aiming)
			{
				DeadEye(true);

				Sequence s = DOTween.Sequence();
				for (int i = 0; i < targets.Count; i++)
				{
					EnemyScript enemy = targets[i].GetComponentInParent<EnemyScript>();
					s.Append(transform.DOLookAt(enemy.transform.position, 0.5f).SetUpdate(true));
					s.AppendCallback(() => anim.SetTrigger("fire"));
					int x = i; //循环缓存栈堆 暂存
					s.AppendInterval(0.05f);
					s.AppendCallback(FirePolish);
					s.AppendCallback(() => enemy.Ragdoll(true, targets[x]));
					s.AppendCallback(() => crossList[x].GetComponent<Image>().color = Color.clear);
					s.AppendInterval(0.35f);
				}

				s.AppendCallback(() => Aim(false));
				s.AppendCallback(() => DeadEye(false));
			}

			if (Input.GetMouseButton(1) == false && aiming == true && deadEye == false)
			{
				Aim(false);
			}
			
			//TODO:DOING
		}

		private void ReLoad()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}
		}


		private void WeaponPosition()
		{
			bool state = input.speed > 0;

			Vector3 pos = state ? gunAimPos : gunIdlePos;
			Vector3 rot = state ? gunAimRot : gunIdleRot;
			gun.DOLocalMove(pos, 0.3f);
			gun.DOLocalRotate(rot, 0.3f);
		}

		private void Aim(bool state)
		{
			aiming = state;

			float xOrigOffset = state ? originalOffsetAmount : zoomOffsetAmount;
			float xCurrentOffset = state ? zoomOffsetAmount : originalOffsetAmount;
			float zoom = state ? 20 : 30;

			DOVirtual.Float(xOrigOffset, xCurrentOffset, aimTime, HorizontalOffset);
			DOVirtual.Float(thirdPersonCam.m_Lens.FieldOfView, zoom, aimTime, CameraZoom);

			anim.SetBool(aiming_ID, state);

			float timeScale = state ? 0.3f : 1f;
			float origTimeScale = state ? 1f : 0.3f;
			DOVirtual.Float(origTimeScale, timeScale, 0.2f, SetTimeScale);

			if (state == false)
			{
				//transform.DOKill();
				transform.DORotate(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z), .2f);
			}

			Vector3 pos = state ? gunAimPos : gunIdlePos;
			Vector3 rot = state ? gunAimRot : gunIdleRot;
			gun.DOComplete();
			gun.DOLocalMove(pos, 0.1f);
			gun.DOLocalRotate(rot, 0.1f);

			float origChromatic = state ? 0.0f : 0.4f;
			float newChromatic = state ? 0.4f : 0.0f;
			currentColor = state ? deadEyeColor : Color.white;

			DOVirtual.Float(origChromatic, newChromatic, 0.1f, AberrationAmount);
			DOVirtual.Float(origChromatic, newChromatic, 0.1f, VignetteAmount);

			Color c = state ? Color.white : Color.clear;
			reticle.color = c;
		}

		private void DeadEye(bool state)
		{
			deadEye = state;

			float animationSpeed = state ? 2 : 1;
			anim.speed = animationSpeed;

			if (state)
			{
				reticle.DOColor(Color.clear, 0.05f);
			}

			if (!state)
			{
				targets.Clear();

				foreach (var t in crossList)
				{
					Destroy(t.gameObject);
				}

				crossList.Clear();
			}

			input.enabled = !state;
		}

		private void FirePolish()
		{
			impulse.GenerateImpulse();

			foreach (var p in gun.GetComponentsInChildren<ParticleSystem>())
			{
				p.Play();
			}
		}


		private void HorizontalOffset(float x)
		{
			for (int i = 0; i < 3; i++)
			{
				CinemachineComposer c = thirdPersonCam.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
				c.m_TrackedObjectOffset.x = x;
			}
		}

		private void CameraZoom(float x)
		{
			thirdPersonCam.m_Lens.FieldOfView = x;
		}

		private void SetTimeScale(float x)
		{
			Time.timeScale = x;
		}

		void AberrationAmount(float x)
		{
			chromaticAberration.intensity.value = x;
		}

		void VignetteAmount(float x)
		{
			vignette.intensity.value = x;
		}
	}
}