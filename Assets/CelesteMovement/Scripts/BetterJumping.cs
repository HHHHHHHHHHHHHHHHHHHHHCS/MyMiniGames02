using System;
using UnityEngine;

namespace CelesteMovement.Scripts
{
	public class BetterJumping : MonoBehaviour
	{
		public float fallMultiplier = 2.5f;
		public float lowJumpMultiplier = 2f;

		private Rigidbody2D rigi;

		private void Start()
		{
			rigi = GetComponent<Rigidbody2D>();
		}

		private void Update()
		{
			if (rigi.velocity.y < 0)
			{
				rigi.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
			}
			else if (rigi.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
			{
				rigi.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
			}
		}
	}
}