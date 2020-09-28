using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace RoyaleShoot
{
	public class PlayerCtrl : MonoBehaviour
	{
		private static readonly int IsMovingID = Animator.StringToHash("IsMoving");
		private static readonly int IsZoomID = Animator.StringToHash("IsZoom");
		private static readonly int IsAttackID = Animator.StringToHash("IsAttack");

		public GameObject arrowPrefab;
		public CanvasGroup canvasGroup;

		public float velocity = 6f;

		public CinemachineFreeLook cvCam;
		public float desiredRotationSpeed = 0.1f;
		public float allowPlayerRotation = 0.1f;
		public float arrowTime = 0.6f;


		public float zoomFOV;
		public Vector3 zoomOffset;

		private Animator anim;
		private Camera cam;
		private CharacterController controller;

		private float inputX;
		private float inputZ;
		private float speed;
		private bool isMoving;
		private bool isZooming;
		private bool blockRotationPlayer;


		private CinemachineComposer[] composers;
		private CinemachineImpulseSource impulse;
		private float normalFOV;
		private Vector3 normalOffset;
		private Tweener zoomTweener;
		private float arrowTimer;

		private void Start()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Locked;

			anim = GetComponent<Animator>();
			cam = Camera.main;
			controller = GetComponent<CharacterController>();

			normalFOV = cvCam.m_Lens.FieldOfView;
			composers = new CinemachineComposer[3];
			for (int i = 0; i < 3; i++)
			{
				composers[i] = cvCam.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
			}

			impulse = cvCam.GetComponent<CinemachineImpulseSource>();

			normalOffset = composers[0].m_TrackedObjectOffset;
		}

		private void Update()
		{
			inputX = Input.GetAxis("Horizontal");
			inputZ = Input.GetAxis("Vertical");

			speed = new Vector2(inputX, inputZ).sqrMagnitude;
			isMoving = speed > allowPlayerRotation;
			isZooming = Input.GetMouseButton(1);

			if (isMoving)
			{
				anim.SetBool(IsMovingID, true);
				PlayerMoveAndRotation(isZooming ? 0.2f : 1f);
			}
			else
			{
				anim.SetBool(IsMovingID, false);
			}

			if (arrowTimer >= 0)
			{
				arrowTimer -= Time.deltaTime;
			}

			bool rightDown = Input.GetMouseButtonDown(1);
			bool rightUp = Input.GetMouseButtonUp(1);
			if (isZooming)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(cam.transform.forward),
					desiredRotationSpeed);

				if (Input.GetMouseButtonUp(0) && arrowTimer < 0)
				{
					anim.SetTrigger(IsAttackID);
					arrowTimer = arrowTime;
					OnShoot();
				}
			}

			if (rightDown || rightUp)
			{
				arrowTimer = arrowTime;
				anim.SetBool(IsZoomID, rightDown);
				Zoom(rightDown);
			}
		}

		private void PlayerMoveAndRotation(float scale)
		{
			var forward = cam.transform.forward;
			var right = cam.transform.right;

			forward.y = 0f;
			right.y = 0f;

			forward.Normalize();
			right.Normalize();

			var desiredMoveDirection = forward * inputZ + right * inputX;
			blockRotationPlayer = isZooming;
			
			if (!blockRotationPlayer)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
					desiredRotationSpeed);
			}
			controller.Move(desiredMoveDirection * (scale * Time.deltaTime * velocity));

		}

		private void Zoom(bool state)
		{
			float from = state ? 0 : 1;
			float to = state ? 1 : 0;

			zoomTweener?.Kill();

			zoomTweener = DOVirtual.Float(from, to, .2f, SetCameraOffset).SetUpdate(true);
		}

		private void SetCameraOffset(float time)
		{
			Vector3 offset = isZooming ? zoomOffset : normalOffset;
			var xy = Vector2.Lerp(composers[0].m_TrackedObjectOffset, offset, time);
			float fov = isZooming ? zoomFOV : normalFOV;

			cvCam.m_Lens.FieldOfView = Mathf.Lerp(cvCam.m_Lens.FieldOfView, fov, time);
			canvasGroup.alpha = time;
			foreach (var c in composers)
			{
				c.m_TrackedObjectOffset.Set(xy.x, xy.y, c.m_TrackedObjectOffset.z);
			}
		}

		private void OnShoot()
		{
			impulse.GenerateImpulse();
			//先放到父物体预设的位置
			var arrow = Instantiate(arrowPrefab, transform.GetChild(0));
			//然后脱离父物体
			arrow.transform.SetParent(null);
		}
	}
}