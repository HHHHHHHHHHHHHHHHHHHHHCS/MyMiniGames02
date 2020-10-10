using System;
using UnityEngine;
using UnityEngine.AI;

namespace RoyaleBattle
{
	public class Unit : ThinkingPlaceable
	{
		private float speed;

		private Animator animator;
		private NavMeshAgent navMeshAgent;

		private void Awake()
		{
			pType = PlaceableType.Unit;

			animator = GetComponent<Animator>();
			navMeshAgent = GetComponent<NavMeshAgent>();
			audioSource = GetComponent<AudioSource>();
		}

		//call by GameManager
		public void Activate(Faction pFaction, PlaceableData pData)
		{
			faction = pFaction;
			hitPoints = pData.hitPoints;
			targetType = pData.targetType;
			attackRange = pData.attackRange;
			attackRatio = pData.attackRatio;
			speed = pData.speed;
			damage = pData.damagePerAttack;
			attackAudioClip = pData.attackClip;
			dieAudioClip = pData.dieClip;
			//TODO:添加更多的属性关联

			navMeshAgent.speed = speed;
			animator.SetFloat("MoveSpeed", speed);

			state = States.Idle;
			navMeshAgent.enabled = true;
		}

		public override void Seek()
		{
			if (target == null)
			{
				return;
			}

			base.Seek();

			navMeshAgent.SetDestination(target.transform.position);
			navMeshAgent.isStopped = false;
			animator.SetBool("IsMoving", true);
		}

		//准备开始攻击动画
		public override void StartAttack()
		{
			base.StartAttack();

			navMeshAgent.isStopped = true;
			animator.SetBool("IsMoving", false);
		}

		//开始攻击
		public override void DealBlow()
		{
			base.DealBlow();

			animator.SetTrigger("Attack");
			transform.forward = (target.transform.position - transform.position).normalized;
		}

		public override void Stop()
		{
			base.Stop();

			navMeshAgent.isStopped = true;
			animator.SetBool("IsMoving", false);
		}

		protected override void Die()
		{
			base.Die();
			navMeshAgent.enabled = false;
			animator.SetTrigger("IsDead");
		}
	}
}