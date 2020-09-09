using System;
using UnityEngine;

namespace BoTWArrow
{
	[ExecuteInEditMode]
	public class ParticleRotation : MonoBehaviour
	{
		public float speed = 10;

		private void Update()
		{
			transform.eulerAngles += new Vector3(0, 0, speed) * Time.deltaTime;
		}
	}
}