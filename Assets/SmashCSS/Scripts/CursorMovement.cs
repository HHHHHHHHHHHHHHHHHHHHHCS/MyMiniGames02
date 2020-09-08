using System;
using UnityEngine;

namespace SmashCSS
{
	public class CursorMovement : MonoBehaviour
	{
		public float speed;

		private Camera mainCamera;

		private void Awake()
		{
			mainCamera = Camera.main;
		}

		private void Update()
		{
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis("Vertical");

			transform.position += new Vector3(x, y, 0) * Time.deltaTime * speed;

			transform.position = new Vector3(Mathf.Clamp(transform.position.x, 0, Screen.width)
				, Mathf.Clamp(transform.position.y, 0, Screen.height)
				, transform.position.z);
		}
	}
}