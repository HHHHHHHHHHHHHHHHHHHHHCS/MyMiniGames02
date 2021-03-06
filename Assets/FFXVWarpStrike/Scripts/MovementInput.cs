using UnityEngine;

namespace FFXVWarpStrike
{
	[RequireComponent(typeof(CharacterController))]
	public class MovementInput : MonoBehaviour
	{
		public bool canMove;

		public float velocity = 9;
		[Space] public Vector3 desiredMoveDirection;
		public bool blockRotationPlayer;
		public float desiredRotationSpeed = 0.1f;
		public float allowPlayerRotation = 0.1f;

		[HideInInspector] public float speed;

		private Camera cam;
		private CharacterController controller;
		private float verticalVel;
		private Vector3 moveVector;

		private void Start()
		{
			cam = Camera.main;
			controller = GetComponent<CharacterController>();
		}

		private void Update()
		{
			speed = 0;
			if (!canMove)
				return;

			InputMagnitude();
		}

		private void InputMagnitude()
		{
			float inputX = Input.GetAxis("Horizontal");
			float inputZ = Input.GetAxis("Vertical");

			speed = new Vector2(inputX, inputZ).sqrMagnitude;

			if (speed > allowPlayerRotation)
			{
				PlayerMoveAndRotation(inputX, inputZ);
			}

			// else if (Speed < allowPlayerRotation)
			// {
			// }
		}

		void PlayerMoveAndRotation(float inputX, float inputZ)
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

		public void RotateTowards(Transform t)
		{
			//like look at
			transform.rotation = Quaternion.LookRotation(t.position - transform.position);
		}
	}
}