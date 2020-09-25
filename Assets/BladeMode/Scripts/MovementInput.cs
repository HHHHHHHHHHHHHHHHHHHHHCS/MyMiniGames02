using UnityEngine;

namespace BladeMode
{
	[RequireComponent(typeof(CharacterController))]
	public class MovementInput : MonoBehaviour
	{
		public float velocity;
		[Space] public float inputX;
		public float inputZ;
		public Vector3 desiredMoveDirection;
		public bool blockRotationPlayer;
		public float desiredRotationSpeed = 0.1f;
		public float speed;
		public float allowPlayerRotation = 0.1f;
		public bool isGrounded;

		[Header("Animation Smoothing")] [Range(0, 1f)]
		public float horizontalAnimSmoothTime = 0.2f;
		[Range(0, 1f)] public float verticalAnimTime = 0.2f;
		
		[Range(0, 1f)] public float StartAnimTime = 0.3f;
		[Range(0, 1f)] public float StopAnimTime = 0.15f;

		private Animator anim;
		private Camera cam;
		private CharacterController controller;

	
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
		}

		private void InputMagnitude()
		{
			inputX = Input.GetAxis("Horizontal");
			inputZ = Input.GetAxis("Vertical");

			//anim.SetFloat ("InputZ", InputZ, VerticalAnimTime, Time.deltaTime * 2f);
			//anim.SetFloat ("InputX", InputX, HorizontalAnimSmoothTime, Time.deltaTime * 2f);

			speed = new Vector2(inputX, inputZ).sqrMagnitude;

			if (speed > allowPlayerRotation)
			{
				anim.SetFloat("Blend", speed, StartAnimTime, Time.deltaTime);
				PlayerMoveAndRotation();
			}
			else if (speed < allowPlayerRotation)
			{
				anim.SetFloat("Blend", speed, StopAnimTime, Time.deltaTime);
			}
		}


		private void PlayerMoveAndRotation()
		{
			var forward = cam.transform.forward;
			var right = cam.transform.right;

			forward.y = 0f;
			right.y = 0f;

			forward.Normalize();
			right.Normalize();

			desiredMoveDirection = forward * inputZ + right * inputX;

			if (blockRotationPlayer == false)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
					desiredRotationSpeed);
				controller.Move(desiredMoveDirection * Time.deltaTime * velocity);
			}
		}


		public void LookAt(Vector3 pos)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), desiredRotationSpeed);
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