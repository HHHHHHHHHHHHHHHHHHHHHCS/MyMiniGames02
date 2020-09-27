using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace RoyaleShoot
{
	public class PlayerCtrl : MonoBehaviour
	{
		private static readonly int IsMovingID = Animator.StringToHash("IsMoving");

		public float velocity = 6f;

		public CinemachineFreeLook cvCam;
		public bool blockRotationPlayer;
		public float desiredRotationSpeed = 0.1f;
		public float allowPlayerRotation = 0.1f;

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

		private CinemachineComposer[] composers;
		private float normalFOV;
		private Vector3 normalOffset;

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

			bool rightDown = Input.GetMouseButtonDown(1);
			bool rightUp = Input.GetMouseButtonUp(1);
			if (isZooming)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(cam.transform.forward),
					desiredRotationSpeed);
			}

			if (rightDown || rightUp)
			{
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

			if (!blockRotationPlayer)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
					desiredRotationSpeed);
				controller.Move(desiredMoveDirection * (scale * Time.deltaTime * velocity));
			}
		}

		private void Zoom(bool state)
		{
			float fov = state ? zoomFOV : normalFOV;
			float from = state ? 0 : 1;
			float to = state ? 1 : 0;


			DOVirtual.Float(cvCam.m_Lens.FieldOfView, fov, .1f, SetFieldOfView);
			DOVirtual.Float(from, to, .2f, SetCameraOffset).SetUpdate(true);
		}

		private void SetFieldOfView(float fov)
		{
			cvCam.m_Lens.FieldOfView = fov;
		}

		private void SetCameraOffset(float time)
		{
			Vector3 offset = isZooming ? zoomOffset : normalOffset;
			var xy = Vector2.Lerp(composers[0].m_TrackedObjectOffset, offset, time);
			foreach (var c in composers)
			{
				c.m_TrackedObjectOffset.Set(xy.x, xy.y, c.m_TrackedObjectOffset.z);
			}
		}
	}
}