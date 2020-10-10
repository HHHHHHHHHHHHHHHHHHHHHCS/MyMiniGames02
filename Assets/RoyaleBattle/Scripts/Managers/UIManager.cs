using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoyaleBattle
{
	public class UIManager : MonoBehaviour
	{
		public GameObject healthBarPrefab;
		public GameObject gameOverUI;

		private List<HealthBar> healthBars;
		private Transform healthBarContainer;

		private void Awake()
		{
			healthBars = new List<HealthBar>();
			healthBarContainer = new GameObject("HealthBarContainer").transform;
		}

		private void LateUpdate()
		{
			foreach (var item in healthBars)
			{
				item.Move();
			}
		}

		public void AddHealthUI(ThinkingPlaceable p)
		{
			GameObject newUIObject = Instantiate(healthBarPrefab, p.transform.position, Quaternion.identity,
				healthBarContainer);
			p.healthBar = newUIObject.GetComponent<HealthBar>();
			p.healthBar.Initialize(p);

			healthBars.Add(p.healthBar);
		}

		public void RemoveHealthUI(ThinkingPlaceable p)
		{
			healthBars.Remove(p.healthBar);
			Destroy(p.healthBar.gameObject);
		}

		public void ShowGameOverUI()
		{
			gameOverUI.SetActive(true);
		}

		public void OnRetryButton()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}