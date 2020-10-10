using UnityEngine;
using UnityEngine.Events;

namespace RoyaleBattle
{
	public class ThinkingPlaceable : Placeable
	{
		//单位状态
		public enum States
		{
			Dragged, //玩家拖动卡的时候
			Idle, //刚放置的时候
			Seeking, //移动目标物体
			Attacking, //攻击
			Dead, //死亡
		}


		//攻击方式的类型
		public enum AttackType
		{
			Melee, //近战
			Ranged, //远程
		}

		[Header("Projectile for Ranged")] public GameObject projectilePrefab;
		public Transform projectileSpawnPoint;

		public UnityAction<ThinkingPlaceable> OnDealDamage, OnProjectileFired;

		[HideInInspector] public States state = States.Dragged;
		[HideInInspector] public AttackType attackType;
		[HideInInspector] public ThinkingPlaceable target;
		[HideInInspector] public HealthBar healthBar;
		[HideInInspector] public float hitPoints; //HP
		[HideInInspector] public float attackRange;
		[HideInInspector] public float attackRatio;
		[HideInInspector] public float lastBlowTime = -1000f;
		[HideInInspector] public float damage;
		[HideInInspector] public AudioClip attackAudioClip;
		[HideInInspector] public float timeToActNext = 0f;

		private Projectile projectile;
		protected AudioSource audioSource;

		public virtual void SetTarget(ThinkingPlaceable t)
		{
			target = t;
			t.OnDie += TargetIsDead;
		}

		public virtual void StartAttack()
		{
			state = States.Attacking;
		}

		//攻击
		public virtual void DealBlow()
		{
			lastBlowTime = Time.time;
		}

		public void DealDamage()
		{
			//只有近战单位攻击的时候,才播放音频
			//if(attackType == AttackType.Melee)
			//audioSource.PlayOneShot(attackAudioClip, 1f);

			if (OnDealDamage != null)
			{
				OnDealDamage(this);
			}
		}

		public void FireProjectile()
		{
			//远程单位在发射炮弹时播放音频
			//audioSource.PlayOneShot(attackAudioClip, 1f);

			if (OnProjectileFired != null)
				OnProjectileFired(this);
		}

		public virtual void Seek()
		{
			state = States.Seeking;
		}

		protected void TargetIsDead(Placeable p)
		{
			state = States.Idle;

			target.OnDie -= TargetIsDead;

			timeToActNext = lastBlowTime + attackRatio;
		}

		public bool IsTargetInRange()
		{
			return (transform.position - target.transform.position).sqrMagnitude <= attackRange * attackRange;
		}

		public float SufferDamage(float amount)
		{
			hitPoints -= amount;

			if (state != States.Dead
			    && hitPoints <= 0f)
			{
				Die();
			}

			return hitPoints;
		}

		public virtual void Stop()
		{
			state = States.Idle;
		}

		protected virtual void Die()
		{
			state = States.Dead;
			
			//audioSource.pitch = Random.Range(.9f, 1.1f);
			//audioSource.PlayOneShot(dieAudioClip, 1f);

			if (OnDie != null)
			{
				OnDie(this);
			}
		}
	}
}