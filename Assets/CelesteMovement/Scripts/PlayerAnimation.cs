using System;
using UnityEngine;

namespace CelesteMovement.Scripts
{
	public class PlayerAnimation : MonoBehaviour
	{
		private static readonly int OnGround_ID = Animator.StringToHash("onGround");
		private static readonly int OnWall_ID = Animator.StringToHash("onWall");
		private static readonly int OnRightWall_ID = Animator.StringToHash("onRightWall");
		private static readonly int WallGrab_ID = Animator.StringToHash("wallGrab");
		private static readonly int WallSlide_ID = Animator.StringToHash("wallSlide");
		private static readonly int CanMove_ID = Animator.StringToHash("canMove");
		private static readonly int IsDashing_ID = Animator.StringToHash("isDashing");
		private static readonly int HorizontalAxis_ID = Animator.StringToHash("HorizontalAxis");
		private static readonly int VerticalAxis_ID = Animator.StringToHash("VerticalAxis");
		private static readonly int VerticalVelocity_ID = Animator.StringToHash("VerticalVelocity");

		[HideInInspector] public SpriteRenderer sr;

		private Animator anim;
		private PlayerMovement move;
		private PlayerCollision coll;


		private void Start()
		{
			anim = GetComponent<Animator>();
			coll = GetComponentInParent<PlayerCollision>();
			move = GetComponentInParent<PlayerMovement>();
			sr = GetComponent<SpriteRenderer>();
		}

		private void Update()
		{
			anim.SetBool(OnGround_ID, coll.OnGround);
			anim.SetBool(OnWall_ID, coll.OnWall);
			anim.SetBool(OnRightWall_ID, coll.OnRightWall);
			anim.SetBool(WallGrab_ID, move.wallGrab);
			anim.SetBool(WallSlide_ID, move.wallSlide);
			anim.SetBool(CanMove_ID, move.canMove);
			anim.SetBool(IsDashing_ID, move.isDashing);
		}

		public void SetHorizontalMovement(float x, float y, float yVel)
		{
			anim.SetFloat(HorizontalAxis_ID, x);
			anim.SetFloat(VerticalAxis_ID, y);
			anim.SetFloat(VerticalVelocity_ID, yVel);
		}

		public void SetTrigger(string trigger)
		{
			anim.SetTrigger(trigger);
		}

		public void Flip(int side)
		{
			if (move.wallGrab || move.wallSlide)
			{
				if ((side == -1 && sr.flipX)
				    || (side == 1 && !sr.flipX))
				{
					return;
				}
			}

			sr.flipX = side == 1;
		}
	}
}