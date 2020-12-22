using System;
using System.Collections.Generic;
using System.Linq;
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
		private static readonly int speed_ID = Animator.StringToHash("speed");

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
		private Color startColor = Color.white;
		private Color endColor = Color.white;


		[Space, Header("Booleans")] private bool aiming; //处于瞄准模式
		private bool deadEye; //处于射击模式

		[Space, Header("Settings")] private float originalZoom;
		public float originalOffsetAmount;
		public float zoomOffsetAmount;
		public float aimTime;
		private List<EnemyScript> targets = new List<EnemyScript>();

		[Space, Header("UI")] public GameObject aimPrefab;
		public Transform canvas;
		public Image reticle;
		private List<Transform> crossList = new List<Transform>();

		[Space, Header("Gun")] public Transform gun;
		public Vector3 gunAimPos = new Vector3(0.3273799f, -0.03389892f, -0.08808608f);
		public Vector3 gunAimRot = new Vector3(-1.763f, -266.143f, -263.152f);
		private Vector3 gunIdlePos;
		private Vector3 gunIdleRot;

		[Space, Header("Enemy")] public LayerMask enemyLayer;
		public string enemyTag = "Enemy";

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

			var mainCam = Camera.main;

			input.canMove = !(aiming || deadEye);

			if (aiming)
			{
				if (targets.Count > 0)
				{
					for (int i = 0; i < targets.Count; i++)
					{
						crossList[i].position = mainCam.WorldToScreenPoint(targets[i].aimingPoint.transform.position);
					}
				}
			}

			//处于射击模式ing 不处理
			if (deadEye)
			{
				return;
			}

			anim.SetFloat(speed_ID, input.speed);

			if (!aiming)
			{
				WeaponPosition();
			}
			else
			{
				input.LookAt(mainCam.transform.forward + (mainCam.transform.right * .1f));
			}

			if (Input.GetMouseButtonDown(1) && !deadEye)
			{
				Aim(true);
				input.LookAt(mainCam.transform.forward + mainCam.transform.right * 0.1f);
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
					s.AppendCallback(() => enemy.Ragdoll(true));
					s.AppendCallback(() => crossList[x].GetComponent<Image>().color = Color.clear);
					s.AppendInterval(0.35f);
				}

				s.AppendCallback(() => Aim(false));
				s.AppendCallback(() => DeadEye(false));
			}

			//防止bug 复位用
			if (Input.GetMouseButton(1) == false && aiming == true && deadEye == false)
			{
				Aim(false);
			}

			//TODO:移动中地方单位是红色 否则是白色
			//TODO:枪的火焰
			//TODO:朝向的跟随
			
			if (aiming && Input.GetMouseButtonDown(0))
			{
				RaycastHit hit;
				Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, float.PositiveInfinity,
					enemyLayer);

				if (!deadEye)
				{
					reticle.color = Color.white;
				}

				if (hit.transform == null)
				{
					return;
				}

				if (!hit.collider.CompareTag(enemyTag))
				{
					return;
				}

				reticle.color = Color.red;

				var enemy = hit.transform.GetComponentInParent<EnemyScript>();

				if (targets.Contains(enemy))
				{
					return;
				}

				if (!enemy.aimed)
				{
					enemy.aimed = true;
					enemy.CreateAimingPoint(hit.transform,hit.point);
					targets.Add(enemy);

					Vector3 convertedPos = mainCam.WorldToScreenPoint(hit.point);
					GameObject cross = Instantiate(aimPrefab, canvas);
					cross.transform.position = convertedPos;
					crossList.Add(cross.transform);
				}
			}
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
			startColor = state ? Color.white : deadEyeColor;
			endColor = state ? deadEyeColor : Color.white;

			DOVirtual.Float(0, 1, 0.1f, ColorAdjustments);
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

		private void ColorAdjustments(float x)
		{
			colorAdjustments.colorFilter.value = Color.Lerp(startColor, endColor, x);
		}

		private void AberrationAmount(float x)
		{
			chromaticAberration.intensity.value = x;
		}

		private void VignetteAmount(float x)
		{
			vignette.intensity.value = x;
		}
	}
}