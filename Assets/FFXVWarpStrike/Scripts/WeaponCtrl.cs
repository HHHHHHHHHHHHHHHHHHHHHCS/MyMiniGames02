using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace FFXVWarpStrike
{
	public class WeaponCtrl : MonoBehaviour
	{
		private const string c_AlphaThreshold = "_AlphaThreshold";
	
		private static readonly int Blend_ID = Animator.StringToHash("blend");
		private static readonly int Slash_ID = Animator.StringToHash("slash");
		private static readonly int Hit_ID = Animator.StringToHash("hit");

		public static WeaponCtrl Instance { get; private set; }


		public bool isLocked;

		[Space] public CinemachineFreeLook cameraFreeLook;
		private CinemachineImpulseSource impulse;

		[Space] public float weaponDuration = 0.5f;
		public Transform sword;
		public Transform swordHand;
		private Vector3 swordOrigRot;
		private Vector3 swordOrigPos;
		private MeshRenderer swordMesh;

		[Space, Header("Volume")] public Volume postVolume;
		private VolumeProfile postProfile;

		[Space, Header("Material")] public Material glowMaterial;

		[Space, Header("Particles")] public ParticleSystem blueTrail;
		public ParticleSystem whiteTrail;
		public ParticleSystem swordParticle;

		[Space, Header("Prefabs")] public GameObject hitParticle;

		[Space, Header("UI")] public Image aim;
		public Image lockAim;
		public Vector2 uiOffset;

		private HashSet<SkillTarget> screenTargets;
		private Transform target;

		private Camera mainCam;
		private MovementInput moveInput;
		private Animator anim;


		private void Awake()
		{
			Instance = this;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			screenTargets = new HashSet<SkillTarget>();
			moveInput = GetComponent<MovementInput>();
			anim = GetComponent<Animator>();
			impulse = cameraFreeLook.GetComponent<CinemachineImpulseSource>();
			mainCam = Camera.main; //2020 之后 camera.main 优化了   但是代码习惯
			postProfile = postVolume.profile;
			swordOrigPos = sword.localPosition;
			swordOrigRot = sword.localEulerAngles;
			swordMesh = sword.GetComponentInChildren<MeshRenderer>();
			swordMesh.enabled = false;
		}

		private void Update()
		{
			if (!Cursor.visible && Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}

			anim.SetFloat(Blend_ID, moveInput.speed);

			if (!moveInput.canMove)
			{
				return;
			}

			if (screenTargets.Count <= 0)
			{
				return;
			}

			if (!isLocked)
			{
				target = TargetIndex()?.transform;
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

				if (Cursor.visible)
				{
					Cursor.visible = false;
					Cursor.lockState = CursorLockMode.Locked;
				}
			}
		}

		private void UserInterface()
		{
			if (!target)
			{
				aim.color = Color.clear;
			}


			var temp = mainCam.WorldToScreenPoint(target.position + (Vector3) uiOffset);
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

		private SkillTarget TargetIndex()
		{
			SkillTarget target = null;
			float minScreenDistance = 0;
			float minWorldDistance = 0;

			foreach (var item in screenTargets)
			{
				if (target == null)
				{
					target = item;
					minScreenDistance = Vector2.Distance(
						mainCam.WorldToScreenPoint(target.transform.position),
						new Vector2(Screen.width / 2f, Screen.height / 2f));
					minWorldDistance = Vector3.Distance(target.transform.position, transform.position);
				}
				else
				{
					float tempDistance = Vector2.Distance(
						mainCam.WorldToScreenPoint(item.transform.position),
						new Vector2(Screen.width / 2f, Screen.height / 2f));
					float tempWD = Vector3.Distance(item.transform.position, transform.position);

					if (minScreenDistance > tempDistance ||
					    (minScreenDistance == tempDistance && minWorldDistance > tempWD))
					{
						target = item;
						minScreenDistance = tempDistance;
						minWorldDistance = tempWD;
					}
				}
			}

			return target;
		}

		public void Warp()
		{
			GameObject clone = GameObject.Instantiate(gameObject, transform.position, transform.rotation);
			Destroy(clone.GetComponent<WeaponCtrl>().sword.gameObject);
			Destroy(clone.GetComponent<Animator>());
			Destroy(clone.GetComponent<WeaponCtrl>());
			Destroy(clone.GetComponent<MovementInput>());
			Destroy(clone.GetComponent<CharacterController>());

			var instantiateMat = Instantiate(glowMaterial);
			instantiateMat.DOFloat(2, c_AlphaThreshold, 5f).OnComplete(() =>
			{
				Destroy(clone);
				Destroy(instantiateMat);
			});
			SkinnedMeshRenderer[] skinMeshList = clone.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var skinnedMeshRenderer in skinMeshList)
			{
				skinnedMeshRenderer.material = instantiateMat;
			}

			ShowBody(false);
			anim.speed = 0;

			transform.DOMove(target.position, weaponDuration).SetEase(Ease.InExpo).OnComplete(FinishWarp);

			sword.parent = null;
			sword.DOMove(target.position, weaponDuration / 1.2f);
			sword.DOLookAt(target.position, 0.2f, AxisConstraint.None);

			blueTrail.Play();
			whiteTrail.Play();

			DOVirtual.Float(0, -0.8f, 0.2f, DistortionAmount);
			DOVirtual.Float(1f, 2f, 0.2f, ScaleAmount);
		}

		private void FinishWarp()
		{
			ShowBody(true);

			sword.parent = swordHand;
			sword.localPosition = swordOrigPos;
			sword.localEulerAngles = swordOrigRot;

			GlowAmount(30);
			DOVirtual.Float(30, 0, 0.5f, GlowAmount);

			//Auto Desotry
			Instantiate(hitParticle, sword.position, Quaternion.identity);

			target.GetComponentInParent<Animator>().SetTrigger(Hit_ID);
			target.parent.DOMove(target.position + transform.forward, 0.5f);

			StartCoroutine(HideSword());
			StartCoroutine(PlayAnimation());
			StartCoroutine(StopParticles());

			isLocked = false;
			LockInterface(false);
			aim.color = Color.clear;

			impulse.GenerateImpulse(Vector3.right);

			DOVirtual.Float(-0.8f, 0, 0.2f, DistortionAmount);
			DOVirtual.Float(2f, 1f, 0.1f, ScaleAmount);
		}

		private IEnumerator PlayAnimation()
		{
			yield return new WaitForSeconds(0.2f);
			anim.speed = 1;
		}

		private IEnumerator StopParticles()
		{
			yield return new WaitForSeconds(0.2f);
			blueTrail.Stop();
			whiteTrail.Stop();
		}

		private IEnumerator HideSword()
		{
			yield return new WaitForSeconds(0.8f);
			swordParticle.Play();

			GameObject swordClone = GameObject.Instantiate(sword.gameObject, sword.position, sword.rotation);
			swordMesh.enabled = false;

			MeshRenderer swordMR = swordClone.GetComponentInChildren<MeshRenderer>();

			var instantiateMat = Instantiate(glowMaterial);
			instantiateMat.DOFloat(1, c_AlphaThreshold, 0.3f).OnComplete(() =>
			{
				Destroy(swordClone);
				Destroy(instantiateMat);
			});

			Material[] materials = new Material[swordMR.sharedMaterials.Length];

			for (int i = 0; i < materials.Length; i++)
			{
				materials[i] = instantiateMat;
			}

			swordMR.materials = materials;

			moveInput.canMove = true;
		}

		private void ShowBody(bool state)
		{
			SkinnedMeshRenderer[] skinMeshList = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var smr in skinMeshList)
			{
				smr.enabled = state;
			}
		}

		private void GlowAmount(float x)
		{
			SkinnedMeshRenderer[] skinMeshList = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var smr in skinMeshList)
			{
				smr.material.SetVector("_FresnelAmount", new Vector4(x, x, x, x));
			}
		}

		private void DistortionAmount(float value)
		{
			postProfile.TryGet(out LensDistortion ld);
			ld.intensity.value = value;
		}

		private void ScaleAmount(float value)
		{
			postProfile.TryGet(out LensDistortion ld);
			ld.scale.value = value;
		}

		public void AddSkillTarget(SkillTarget st)
		{
			screenTargets.Add(st);
		}

		public void RemoveSkillTarget(SkillTarget st)
		{
			screenTargets.Remove(st);
		}
	}
}