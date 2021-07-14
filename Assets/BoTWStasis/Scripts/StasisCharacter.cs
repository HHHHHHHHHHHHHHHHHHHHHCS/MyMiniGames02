using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoTWStasis.Scripts
{
	public class StasisCharacter : MonoBehaviour
	{
		private static readonly int Attacking_ID = Animator.StringToHash("attacking");

		private static readonly int EmissionColor_ID = Shader.PropertyToID("_EmissionColor");
		private static readonly int StasisAmount_ID = Shader.PropertyToID("_StasisAmount");

		private static readonly int Slash_ID = Animator.StringToHash("slash");

		private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

		public static StasisCharacter instance;

		[Header("Collision")] public LayerMask layerMask;

		private MovementInput input;
		public Animator anim;

		[Space] [Header("Aim and Zoom")] public bool stasisAim;
		public CinemachineFreeLook thirdPersonCamera;
		public float zoomDuration = 0.3f;
		private float originalFOV;
		public float zoomFOV;
		private Vector3 originalCameraOffset;
		public Vector3 zoomCameraOffset;

		[Space] [Header("Target")] public Transform target;

		[Space] [Header("Colors")] public Color highlightedColor;
		public Color normalColor;
		public Color finalColor;

		[Space] [Header("Arrow")] public GameObject arrow;

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			input = GetComponent<MovementInput>();
			anim = GetComponent<Animator>();
			originalFOV = thirdPersonCamera.m_Lens.FieldOfView;
			originalCameraOffset = thirdPersonCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>()
				.m_TrackedObjectOffset;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(1))
			{
				StasisAim(true);
			}

			if (Input.GetMouseButtonUp(1))
			{
				StasisAim(false);
			}

			if (stasisAim)
			{
				RaycastHit hit;
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
				{
					if (target != hit.transform)
					{
						if (target != null)
						{
							var oldSO = target.GetComponent<StasisObject>();
							if (oldSO != null)
							{
								oldSO.SetEmissionColor(normalColor);
							}
						}

						var so = hit.transform.GetComponent<StasisObject>();
						if (so != null)
						{
							target = hit.transform;
							if (!so.activated)
							{
								so.SetEmissionColor(highlightedColor);
							}

							AimUIAnimation.instance.Target(true);
						}
						else
						{
							target = null;
							AimUIAnimation.instance.Target(false);
						}
					}
				}
				else
				{
					if (target != null)
					{
						var so = target.GetComponent<StasisObject>();
						if (so != null)
						{
							if (!so.activated)
							{
								so.SetEmissionColor( normalColor);
							}
						}

						target = null;
						AimUIAnimation.instance.Target(false);
					}
				}
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (!stasisAim)
				{
					anim.SetTrigger(Slash_ID);
					StartCoroutine(WaitFrame());
				}
				else
				{
					if (target != null)
					{
						var so = target.GetComponent<StasisObject>();
						if (so)
						{
							bool stasis = so.activated;
							so.SetStasis(!stasis);
							StasisAim(false);
						}
					}
				}
			}

			RestartHotkey();
		}

		private IEnumerator WaitFrame()
		{
			yield return waitForEndOfFrame;
			if (!anim.GetBool(Attacking_ID))
			{
				anim.SetBool(Attacking_ID, true);
			}
		}

		private void StasisAim(bool state)
		{
			stasisAim = state;
			float fov = state ? zoomFOV : originalFOV;
			Vector3 offset = state ? zoomCameraOffset : originalCameraOffset;
			float stasisEffect = state ? 0.4f : 0f;

			CinemachineComposer composer = thirdPersonCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>();
			DOVirtual.Float(thirdPersonCamera.m_Lens.FieldOfView, fov, zoomDuration, SetFieldOfView);
			DOVirtual.Float(composer.m_TrackedObjectOffset.x, offset.x, zoomDuration, SetCameraOffsetX);
			DOVirtual.Float(composer.m_TrackedObjectOffset.y, offset.y, zoomDuration, SetCameraOffsetY);

			var sos = FindObjectsOfType<StasisObject>();
			foreach (var item in sos)
			{
				if (!item.activated)
				{
					var mat = item.GetComponent<Renderer>().material;
					mat.SetColor(EmissionColor_ID, normalColor);
					mat.SetFloat(StasisAmount_ID, stasisEffect);
				}
			}

			AimUIAnimation.instance.Show(state);
		}


		private void SetFieldOfView(float x)
		{
			thirdPersonCamera.m_Lens.FieldOfView = x;
		}

		private void SetCameraOffsetX(float x)
		{
			for (int i = 0; i < 3; i++)
			{
				thirdPersonCamera.GetRig(i).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = x;
			}
		}

		private void SetCameraOffsetY(float y)
		{
			for (int i = 0; i < 3; i++)
			{
				thirdPersonCamera.GetRig(i).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = y;
			}
		}

		private void RestartHotkey()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}
		}
	}
}