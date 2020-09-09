using System;
using UnityEngine;

namespace BoTWArrow
{
	public class RotationTest : MonoBehaviour
	{
		public Vector2 rotationLimit;

		[Range(1, 10)] public float sensitivity;

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);

			transform.eulerAngles = new Vector3(ClampAngle(transform.eulerAngles.x, rotationLimit.x, rotationLimit.y)
				, transform.eulerAngles.y, transform.eulerAngles.z);
		}

		private float ClampAngle(float angle, float from, float to)
		{
			// eg. -80 ,80
			if (angle < 0f)
			{
				angle = 360 + angle;
			}

			if (angle > 180f)
			{
				return Mathf.Max(angle, 360 + from);
			}

			return Mathf.Min(angle, to);
		}
	}
}