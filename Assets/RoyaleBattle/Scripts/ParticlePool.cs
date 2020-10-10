using System;
using UnityEngine;

namespace RoyaleBattle
{
	public class ParticlePool : MonoBehaviour
	{
		public GameObject effectPrefab;
		public int amount = 10;

		private ParticleSystem[] pool;
		private int currentSystem = 0;

		private void Awake()
		{
			pool = new ParticleSystem[amount];
			for (int i = 0; i < amount; i++)
			{
				pool[i] = GameObject.Instantiate<GameObject>(effectPrefab, this.transform)
					.GetComponent<ParticleSystem>();
			}
		}

		public void UseParticles(Vector3 pos)
		{
			currentSystem = (currentSystem + 1 >= pool.Length) ? 0 : currentSystem;

			pool[currentSystem].transform.position = pos;
			pool[currentSystem].Play();
		}
	}
}