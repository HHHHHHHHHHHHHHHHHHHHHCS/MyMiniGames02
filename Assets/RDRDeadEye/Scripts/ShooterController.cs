using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RDRDeadEye
{
	public class ShooterController : MonoBehaviour
	{
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


		[Space, Header("Booleans")] public bool aiming;
		public bool deadEye;

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

			if (deadEye)
			{
				return;
			}

			anim.SetFloat("speed", input.speed);


			ReLoad();
		}

		private bool ReLoad()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
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
	}
}