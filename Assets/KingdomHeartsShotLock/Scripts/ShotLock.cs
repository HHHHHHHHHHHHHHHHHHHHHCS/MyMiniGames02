using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace KingdomHeartsShotLock.Scripts
{
	public class ShotLock : MonoBehaviour
	{
		public PlayableDirector director;
		public bool cinematic;

		[Header("Targets")] public TargetDetection detection;
		public List<Transform> finalTargets = new List<Transform>();


		[Space, Header("Aim and Zoom")] public Transform weaponTip;

		[Space] public bool aiming;
		public CinemachineFreeLook thirdPersonCamera;
		public float zoomDuration = 0.3f;
		public float zoomFov;
		public Vector3 zoomCameraOffset;


		[Space, Header("Prefab")] public GameObject projectilePrefab;

		private float originFOV;
		private Vector3 originalLCameraOffset;

		private Animator anim;

		private KingdomHeartsShotLockMovementInput input;

		//todo:
		// private InterfaceAnimator ui;
		// private Volume volume;

		private float time;
		private int index;
		private int limit = 25;
		private static readonly int PosY_ID = Animator.StringToHash("PosY");

		private void Start()
		{
			Cursor.visible = false;
			// ui = GetComponent<InterfaceAnimator>();
			anim = GetComponent<Animator>();
			input = GetComponent<KingdomHeartsShotLockMovementInput>();
			originFOV = thirdPersonCamera.m_Lens.FieldOfView;
			originalLCameraOffset = thirdPersonCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>()
				.m_TrackedObjectOffset;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}

			if (Input.GetMouseButtonDown(1))
			{
				var intensity = VolumeManager.instance.stack.GetComponent<Vignette>().intensity.value;
				DOVirtual.Float(intensity, 0.8f, 0.2f, SetVignette);
				Aim(true);
			}

			if (Input.GetMouseButtonUp(1))
			{
				var intensity = VolumeManager.instance.stack.GetComponent<Vignette>().intensity.value;
				DOVirtual.Float(intensity, 0.0f, 0.2f, SetVignette);
				if (finalTargets.Count > 0)
				{
					director.Play();
					input.anim.SetFloat(PosY_ID, 1);
					input.enabled = false;
					cinematic = true;
					transform.position += Vector3.up * 3;

					//TODO:LockFollowUI
				}

				Aim(false);
			}


			if (aiming)
			{
				if (time >= 5)
				{
					time = 0;

					List<Transform> oldTargets = new List<Transform>();
					oldTargets = detection.targets;

					if (oldTargets.Count > 0 && finalTargets.Count < limit)
					{
						if (index < oldTargets.Count)
						{
							//TODO:UI
							finalTargets.Add(oldTargets[index]);
						}

						index = Mathf.Min(oldTargets.Count - 1, index + 1);
						if (index == oldTargets.Count - 1)
						{
							index = 0;
						}
					}
				}
				else
				{
					time++;
				}
			}
		}

		public void ActivateShotLock()
		{
			float angle = (360.0f / finalTargets.Count);

			for (int i = 0; i < finalTargets.Count; i++)
			{
				float z = angle * (i + 1);
				Vector3 cam = Camera.main.transform.eulerAngles;
				GameObject projectile = GameObject.Instantiate(projectilePrefab, weaponTip.transform.position,
					Quaternion.Euler(cam.x, cam.y, z));
				//todo:ProjectileScript
				projectile.GetComponent<ProjectileScript>()
			}
		}

		public void Aim(bool state)
		{
			//todo:ui

			if (!state && !cinematic)
			{
				StopAllCoroutines();
				detection.targets.Clear();
				finalTargets.Clear();
				index = 0;
			}

			detection.SetCollider(state);
			aiming = state;
			float foov = state ? zoomFov : originFOV;
			
		}

		public void TargetState(Transform otherTransform, bool b)
		{
			throw new System.NotImplementedException();
		}
	}
}