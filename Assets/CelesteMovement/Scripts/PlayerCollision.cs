using System;
using UnityEngine;

namespace CelesteMovement.Scripts
{
	public class PlayerCollision : MonoBehaviour
	{
		[Header("Layers")] public LayerMask groundLayer;

		[Space] [Header("Collision")] public float collisionRadius = 0.25f;
		public Vector2 bottomOffset, rightOffset, leftOffset;

		public bool OnGround { get; private set; }
		public bool OnWall { get; private set; }
		public bool OnRightWall { get; private set; }
		public bool OnLeftWall { get; private set; }
		public int WallSide { get; private set; }

		public PlayerCollision(bool onGround)
		{
			OnGround = onGround;
		}

		private void FixedUpdate()
		{
			var pos = new Vector2(transform.position.x, transform.position.y);
			OnGround = Physics2D.OverlapCircle(pos + bottomOffset, collisionRadius, groundLayer);

			OnRightWall = Physics2D.OverlapCircle(pos + rightOffset, collisionRadius, groundLayer);
			OnLeftWall = Physics2D.OverlapCircle(pos + leftOffset, collisionRadius, groundLayer);

			OnWall = OnRightWall || OnLeftWall;

			WallSide = OnRightWall ? -1 : 1;
		}

		private void OnDrawGizmos()
		{
			var oldCol = Gizmos.color;
			Gizmos.color = Color.red;

			var pos = transform.position;
			Gizmos.DrawWireSphere(pos + new Vector3(bottomOffset.x, bottomOffset.y, 0), collisionRadius);
			Gizmos.DrawWireSphere(pos + new Vector3(rightOffset.x, rightOffset.y, 0), collisionRadius);
			Gizmos.DrawWireSphere(pos + new Vector3(leftOffset.x, leftOffset.y, 0), collisionRadius);


			Gizmos.color = oldCol;
		}
	}
}