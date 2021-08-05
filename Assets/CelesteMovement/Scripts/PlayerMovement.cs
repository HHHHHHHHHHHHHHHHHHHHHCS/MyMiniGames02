using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CelesteMovement.Scripts
{
	public class PlayerMovement : MonoBehaviour
	{
		public GhostTrail ghostTrail;

		[Header("Stats")] public float speed = 10;
		public float jumpForce = 50;
		public float slideSpeed = 5;
		public float wallJumpLerp = 10;
		public float dashSpeed = 20;

		[Space, Header("Booleans")] public bool canMove;
		public bool wallGrab;
		public bool wallJumped;
		public bool wallSlide;
		public bool isDashing;

		public int side = 1;

		[Space, Header("Polish")] public ParticleSystem dashParticle;
		public ParticleSystem jumpParticle;
		public ParticleSystem wallJumpParticle;
		public ParticleSystem slideParticle;

		private bool groundTouch;
		private bool hasDashed;

		private Transform camTS;
		private PlayerAnimation anim;
		private PlayerCollision coll;
		private Rigidbody2D rigi;
		private BetterJumping betJump;
		private RippleEffect rippleEffect;

		private Coroutine moveCoroutine;

		private void Start()
		{
			coll = GetComponent<PlayerCollision>();
			rigi = GetComponent<Rigidbody2D>();
			anim = GetComponentInChildren<PlayerAnimation>();
			betJump = GetComponent<BetterJumping>();
			camTS = Camera.main.transform;
			rippleEffect = camTS.GetComponent<RippleEffect>();
		}

		private void Update()
		{
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis("Vertical");
			float xRaw = Input.GetAxisRaw("Horizontal");
			float yRaw = Input.GetAxisRaw("Vertical");

			var dir = new Vector2(x, y);

			Walk(dir);
			anim.SetHorizontalMovement(x, y, rigi.velocity.y);

			if (coll.OnWall && Input.GetKey(KeyCode.LeftShift) && canMove)
			{
				if (side != coll.WallSide)
				{
					anim.Flip(side * -1);
				}

				wallGrab = true;
				wallSlide = false;
			}

			if (Input.GetKeyUp(KeyCode.LeftShift) || !coll.OnWall || !canMove)
			{
				wallGrab = false;
				wallSlide = false;
			}

			if (coll.OnGround && !isDashing)
			{
				wallJumped = false;
				betJump.enabled = true;
			}

			if (wallGrab && !isDashing)
			{
				rigi.gravityScale = 0;
				// if (x > 0.2f || x < -0.2f)
				// {
				// 	rigi.velocity = new Vector2(rigi.velocity.x, 0);
				// }

				float speedModifier = y > 0 ? 0.5f : 1;

				rigi.velocity = new Vector2(rigi.velocity.x, y * (speed * speedModifier));
			}
			else
			{
				rigi.gravityScale = 3;
			}

			if (coll.OnWall && !coll.OnGround)
			{
				if (x != 0 && !wallGrab)
				{
					wallSlide = true;
					WallSlide();
				}
			}

			if (!coll.OnWall || coll.OnGround)
			{
				wallSlide = false;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				anim.SetTrigger("jump");

				if (coll.OnGround)
				{
					Jump(Vector2.up, false);
				}
				else if (coll.OnWall)
				{
					WallJump();
				}
			}

			if (Input.GetKeyDown(KeyCode.Z) && !hasDashed)
			{
				if (xRaw != 0 || yRaw != 0)
				{
					Dash(xRaw, yRaw);
				}
			}

			if (coll.OnGround && !groundTouch)
			{
				GroundTouch();
				groundTouch = true;
			}

			if (!coll.OnGround && groundTouch)
			{
				groundTouch = false;
			}

			WallParticle(y);

			if (wallGrab || wallSlide || !canMove)
			{
				return;
			}

			if (x > 0)
			{
				side = 1;
				anim.Flip(side);
			}
			if (x < 0)			
			{
				side = -1;
				anim.Flip(side);
			}
		}

		private void GroundTouch()
		{
			hasDashed = false;
			isDashing = false;

			side = anim.sr.flipX ? -1 : 1;

			jumpParticle.Play();
		}

		private void Dash(float x, float y)
		{
			camTS.DOComplete();
			camTS.DOShakePosition(0.2f, 0.5f, 14, 90, false, true);
			rippleEffect.Emit(Camera.main.WorldToViewportPoint(transform.position));

			hasDashed = true;

			anim.SetTrigger("dash");

			rigi.velocity = Vector2.zero;
			var dir = new Vector2(x, y);

			rigi.velocity += dir.normalized * dashSpeed;
			StartCoroutine(DashWait());
		}

		private IEnumerator DashWait()
		{
			ghostTrail.ShowGhost();
			StartCoroutine(GroundDash());
			DOVirtual.Float(14, 0, 0.8f, RigidbodyDrag);

			dashParticle.Play();
			rigi.gravityScale = 0;
			betJump.enabled = false;
			wallJumped = true;
			isDashing = true;

			yield return new WaitForSeconds(0.3f);

			dashParticle.Stop();

			rigi.gravityScale = 3;
			betJump.enabled = true;
			wallJumped = false;
			isDashing = false;
		}


		private void RigidbodyDrag(float x)
		{
			rigi.drag = x;
		}

		private IEnumerator GroundDash()
		{
			yield return new WaitForSeconds(0.15f);
			if (coll.OnGround)
			{
				hasDashed = false;
			}
		}

		private void WallJump()
		{
			if ((side == 1 && coll.OnRightWall) || (side == -1 && !coll.OnRightWall))
			{
				side *= -1;
				anim.Flip(side);
			}

			if (moveCoroutine != null)
			{
				StopCoroutine(moveCoroutine);
			}

			moveCoroutine = StartCoroutine(DisableMovement(0.1f));

			Vector2 wallDir = coll.OnRightWall ? Vector2.left : Vector2.right;

			Jump((Vector2.up + wallDir) / 1.5f, true);

			wallJumped = true;
		}

		private void Walk(Vector2 dir)
		{
			if (!canMove)
			{
				return;
			}

			if (wallGrab)
			{
				return;
			}

			if (!wallJumped)
			{
				rigi.velocity = new Vector2(dir.x * speed, rigi.velocity.y);
			}
			else
			{
				rigi.velocity = Vector2.Lerp(rigi.velocity, new Vector2(dir.x * speed, rigi.velocity.y),
					wallJumpLerp * Time.deltaTime);
			}
		}

		private void WallSlide()
		{
			if (coll.WallSide != side)
			{
				anim.Flip(side * -1);
			}

			if (!canMove)
			{
				return;
			}

			bool pushingWall = (rigi.velocity.x > 0 && coll.OnRightWall) || (rigi.velocity.x < 0 && coll.OnLeftWall);

			float push = pushingWall ? 0 : rigi.velocity.x;

			rigi.velocity = new Vector2(push, -slideSpeed);
		}

		private void Jump(Vector2 dir, bool wall)
		{
			slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
			ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

			rigi.velocity = new Vector2(rigi.velocity.x, 0);
			rigi.velocity += dir * jumpForce;

			particle.Play();
		}

		private IEnumerator DisableMovement(float time)
		{
			canMove = false;
			yield return new WaitForSeconds(time);
			canMove = true;
		}

		private void WallParticle(float vertical)
		{
			var main = slideParticle.main;

			if (wallSlide || (wallGrab && vertical < 0))
			{
				if (!slideParticle.isPlaying)
				{
					slideParticle.Play();
				}

				slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
				main.startColor = Color.white;
			}
			else
			{
				main.startColor = Color.clear;
				slideParticle.Stop();
			}
		}

		private int ParticleSide()
		{
			return coll.OnRightWall ? 1 : -1;
		}
	}
}