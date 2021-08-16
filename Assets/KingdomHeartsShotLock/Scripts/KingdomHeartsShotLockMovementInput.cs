using System;
using UnityEngine;

namespace KingdomHeartsShotLock.Scripts
{
	[RequireComponent(typeof(CharacterController))]
	public class KingdomHeartsShotLockMovementInput : MonoBehaviour
	{
		private static readonly int PosY_ID = Animator.StringToHash("PosY");
		private static readonly int Blend_ID = Animator.StringToHash("Blend");

		public float velocity;

		[Space] public float inputX;
		public float inputZ;
		public Vector3 desiredMoveDirection;
		public bool blockRotationPlayer;
		public float desiredRotationSpeed = 0.1f;
		public Animator anim;
		public float speed;
		public float allowPlayerRotation;
		public Camera cam;
		public CharacterController controller;

		[Header("Animation Smoothing")] [Range(0, 1f)]
		public float startAnimTime = 0.3f;

		[Range(0, 1f)] public float stopAnimTime = 0.15f;


		private float verticalVel;
		private Vector3 moveVector;
		private float floorY;

		private void Start()
		{
			cam = Camera.main;
			anim = GetComponent<Animator>();
			controller = GetComponent<CharacterController>();
			floorY = transform.position.y;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			InputMagnitude();
		}

		private void InputMagnitude()
		{
			inputX = Input.GetAxis("Horizontal");
			inputZ = Input.GetAxis("Vertical");


			speed = new Vector2(inputX, inputZ).sqrMagnitude;

			anim.SetFloat(PosY_ID, transform.position.y - floorY);

			if (speed > allowPlayerRotation)
			{
				anim.SetFloat(Blend_ID, speed, startAnimTime, Time.deltaTime);
				PlayerMoveAndRotation();
			}
			else if (speed < allowPlayerRotation)
			{
				anim.SetFloat(Blend_ID, speed, stopAnimTime, Time.deltaTime);
			}
		}

		private void PlayerMoveAndRotation()
		{
			var forward = cam.transform.forward;
			var right = cam.transform.right;

			forward.y = 0;
			right.y = 0;

			forward.Normalize();
			right.Normalize();

			desiredMoveDirection = forward * inputZ + right * inputX;

			if (!blockRotationPlayer)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
					desiredRotationSpeed);
				controller.Move(desiredMoveDirection * Time.deltaTime * velocity);
			}
		}
	}
}