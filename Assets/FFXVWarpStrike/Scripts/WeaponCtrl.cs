using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace FFXVWarpStrike
{
	public class WeaponCtrl : MonoBehaviour
	{
		private static readonly int Blend_ID = Animator.StringToHash("Blend");
		private static readonly int Slash_ID = Animator.StringToHash("slash");


		public bool isLocked;

		[Space] public CinemachineFreeLook cameraFreeLook;
		private CinemachineImpulseSource impulse;

		[Space] public float weaponDuration = 0.5f;
		public Transform sword;
		public Transform swordHand;
		private Vector3 swordOrigRot;
		private Vector3 swordOrigPos;
		private MeshRenderer swordMesh;

		[Space, Header("Material")] public Material glowMaterial;

		[Space, Header("Particles")] public ParticleSystem blueTrail;
		public ParticleSystem whiteTrail;
		public ParticleSystem swordParticle;

		[Space, Header("Prefabs")] public GameObject hitParticle;

		[Space, Header("UI")] public Image aim;
		public Image lockAim;
		public Vector2 uiOffset;

		private List<SkillTarget> screenTargets;
		private Transform target;

		private Camera mainCamera;
		private MovementInput moveInput;
		private Animator anim;
		private Volume postVolume;
		private VolumeProfile postProfile;

		private void Awake()
		{
			Cursor.visible = true;

			screenTargets = FindObjectsOfType<SkillTarget>().ToList();

			moveInput = GetComponent<MovementInput>();
			anim = GetComponent<Animator>();
			impulse = cameraFreeLook.GetComponent<CinemachineImpulseSource>();
			mainCamera = Camera.main;
			postVolume = mainCamera.GetComponent<Volume>();
			postProfile = postVolume.profile;
			swordOrigPos = sword.localPosition;
			swordOrigRot = sword.localEulerAngles;
			swordMesh = sword.GetComponentInChildren<MeshRenderer>();
			swordMesh.enabled = false;
		}

		private void Update()
		{
			anim.SetFloat(Blend_ID, moveInput.speed);

			if (!moveInput.canMove)
			{
				return;
			}

			if (screenTargets != null && screenTargets.Count <= 0)
			{
				return;
			}

			if (!isLocked)
			{
				target = screenTargets[TargetIndex()].transform;
			}

			UserInterface();

			if (Input.GetMouseButtonDown(1))
			{
				LockInterface(true);
				isLocked = true;
			}

			if (Input.GetMouseButtonUp(1) && moveInput.canMove)
			{
				LockInterface(false);
				isLocked = false;
			}

			if (!isLocked)
			{
				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				moveInput.RotateTowards(target);
				moveInput.canMove = false;
				swordParticle.Play();
				swordMesh.enabled = true;
				anim.SetTrigger(Slash_ID);
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.visible = !Cursor.visible;
			}
		}

		private void UserInterface()
		{
			if (!target)
			{
				aim.color = Color.clear;
				return;
			}


			var temp = mainCamera.WorldToScreenPoint(target.position + (Vector3) uiOffset);
			if (temp.z > 0)
			{
				//z=0优化合批
				temp.z = 0;
				aim.transform.position = temp;
				aim.color = Color.white;
			}
			else
			{
				//看不见就算了
				aim.color = Color.clear;
			}
		}

		private void LockInterface(bool state)
		{
			float size = state ? 1 : 2;
			float fade = state ? 1 : 0;
			lockAim.DOFade(fade, 0.15f);
			lockAim.transform.DOScale(size, 0.15f).SetEase(Ease.OutBack);
			lockAim.transform.DORotate(Vector3.forward * 180, 0.15f, RotateMode.FastBeyond360).From();
			aim.transform.DORotate(Vector3.forward * 90f, 0.15f, RotateMode.LocalAxisAdd);
		}

		private int TargetIndex()
		{
			float minScreenDistance = Vector2.Distance(
				mainCamera.WorldToScreenPoint(screenTargets[0].transform.position),
				new Vector2(Screen.width / 2f, Screen.height / 2f));
			float minWorldDistance = Vector3.Distance(screenTargets[0].transform.position, transform.position);
			int index = 0;

			for (int i = 1; i < screenTargets.Count; i++)
			{
				float tempDistance = Vector2.Distance(
					mainCamera.WorldToScreenPoint(screenTargets[i].transform.position),
					new Vector2(Screen.width / 2f, Screen.height / 2f));

				if (minScreenDistance > tempDistance)
				{
					minScreenDistance = tempDistance;
					index = i;
				}
				else if (minScreenDistance == tempDistance)
				{
					float wd = Vector3.Distance(screenTargets[i].transform.position, transform.position);
					if (minWorldDistance > wd)
					{
						minWorldDistance = wd;
						index = i;
					}
				}
			}

			return index;
		}

		public void Warp()
		{
			GameObject clone = GameObject.Instantiate(gameObject, transform.position, transform.rotation);
			Destroy(clone.GetComponent<WeaponCtrl>().sword.gameObject);
			Destroy(clone.GetComponent<Animator>());
			Destroy(clone.GetComponent<WeaponCtrl>());
			Destroy(clone.GetComponent<MovementInput>());
			Destroy(clone.GetComponent<CharacterController>());

			SkinnedMeshRenderer[] skinMeshList = clone.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var skinnedMeshRenderer in skinMeshList)
			{
				skinnedMeshRenderer.material = glowMaterial;
				skinnedMeshRenderer.material.DOFloat(2, "_AlphaThreshold", 5f).OnComplete(() => Destroy(clone));
			}

			ShowBody(false);
			anim.speed = 0;

			transform.DOMove(target.position, weaponDuration).SetEase(Ease.InExpo).OnComplete(FinishWarp);

			sword.parent = null;
			sword.DOMove(target.position, weaponDuration / 1.2f);
			sword.DOLookAt(target.position, 0.2f, AxisConstraint.None);

			blueTrail.Play();
			whiteTrail.Play();

			DOVirtual.Float(0, -80, 0.2f, DistortionAmount);
			DOVirtual.Float(1f, 2f, 0.2f, ScaleAmount);
		}

		private void FinishWarp()
		{
			ShowBody(true);

			sword.parent = swordHand;
			sword.localPosition = swordOrigPos;
			sword.localEulerAngles = swordOrigRot;

			SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
			{
				GlowAmount(30);
				DOVirtual.Float(30, 0, 0.5f, GlowAmount);
			}

			//Auto Desotry
			Instantiate(hitParticle, sword.position, Quaternion.identity);

			StartCoroutine(HideSword());
			StartCoroutine(PlayAnimation());
			StartCoroutine(StopPartciles());

			isLocked = false;
			LockInterface(false);
			aim.color = Color.clear;

			impulse.GenerateImpulse(Vector3.right);

			DOVirtual.Float(-80, 0, 0.2f, DistortionAmount);
			DOVirtual.Float(2f, 1f, 0.1f, ScaleAmount);
		}

		private IEnumerator PlayAnimation()
		{
			throw new System.NotImplementedException();
		}
		
		private IEnumerator StopPartciles()
		{
			throw new System.NotImplementedException();
		}

		private IEnumerator HideSword()
		{
			throw new System.NotImplementedException();
		}

		private void ShowBody(bool b)
		{
			throw new System.NotImplementedException();
		}
		
		private void GlowAmount(float i)
		{
			throw new System.NotImplementedException();
		}



		private void DistortionAmount(float value)
		{
		}

		private void ScaleAmount(float value)
		{
		}
	}
}