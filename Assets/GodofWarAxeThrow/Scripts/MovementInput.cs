using UnityEngine;

namespace GodofWarAxeThrow
{
	[RequireComponent(typeof(CharacterController))]
	public class MovementInput : MonoBehaviour
	{
		public float InputX;
		public float InputZ;
		public Vector3 desiredMoveDirection;
		public bool blockRotationPlayer;
		public float desiredRotationSpeed = 0.1f;
		public Animator anim;
		public float speed;
		public float allowPlayerRotation = 0.1f;
		public Camera cam;
		public CharacterController controller;
		public bool isGrounded;

		[Header("Animation Smoothing")] [Range(0, 1f)]
		public float horizontalAnimSmoothTime = 0.2f;

		[Range(0, 1f)] public float verticalAnimTime = 0.2f;
		[Range(0, 1f)] public float startAnimTime = 0.2f;
		[Range(0, 1f)] public float stopAnimTime = 0.15f;

		private float verticalVel;
		private Vector3 moveVector;

		private void Start()
		{
			anim = GetComponent<Animator>();
			cam = Camera.main;
			controller = GetComponent<CharacterController>();
		}


		private void Update()
		{
			InputMagnitude();
			/*
			//If you don't need the character grounded then get rid of this part.
			isGrounded = controller.isGrounded;
			if (isGrounded) {
				verticalVel -= 0;
			} else {
				verticalVel -= 2;
			}
			moveVector = new Vector3 (0, verticalVel, 0);
			controller.Move (moveVector);
			*/
			//Updater
		}

		private void InputMagnitude()
		{
			InputX = Input.GetAxis("Horizontal");
			InputZ = Input.GetAxis("Vertical");

			speed = new Vector2(InputX, InputZ).sqrMagnitude;

			if (speed > allowPlayerRotation)
			{
				PlayerMoveAndRotation();
			}
			else if (speed < allowPlayerRotation)
			{
			}
		}

		private void PlayerMoveAndRotation()
		{
			InputX = Input.GetAxis("Horizontal");
			InputZ = Input.GetAxis("Vertical");

			var forward = cam.transform.forward;
			var right = cam.transform.right;

			forward.y = 0f;
			right.y = 0f;

			forward.Normalize();
			right.Normalize();

			desiredMoveDirection = forward * InputZ + right * InputX;

			if (GetComponent<ThrowController>().aiming == null)
			{
				return;
			}

			if (blockRotationPlayer == false)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
					desiredRotationSpeed);
				controller.Move(desiredMoveDirection * Time.deltaTime * 3);
			}
		}

		public void RotateToCamera(Transform t)
		{
			var forward = cam.transform.forward;
			var right = cam.transform.right;

			desiredMoveDirection = forward;

			t.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
				desiredRotationSpeed);
		}
	}
}