using Cinemachine;
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

		private SkillTarget[] screenTargets;
		private Transform target;

		private Camera mainCamera;
		private MovementInput moveInput;
		private Animator anim;
		private Volume postVolume;
		private VolumeProfile postProfile;

		private void Awake()
		{
			Cursor.visible = true;

			screenTargets = FindObjectsOfType<SkillTarget>();

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

			if (screenTargets != null && screenTargets.Length <= 0)
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

		private void LockInterface(bool b)
		{
			throw new System.NotImplementedException();
		}


		private int TargetIndex()
		{
			throw new System.NotImplementedException();
		}
	}
}