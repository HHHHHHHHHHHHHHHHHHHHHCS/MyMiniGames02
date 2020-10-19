using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoyaleBattle
{
	public class UIManager : MonoBehaviour
	{
		public Transform uiRoot;
		public GameObject healthBarPrefab;
		public GameObject gameOverUI;

		private Camera mainCamera;
		private List<HealthBar> healthBars;
		private Transform healthBarContainer;

		private void Awake()
		{
			mainCamera = Camera.main;

			healthBars = new List<HealthBar>();
			healthBarContainer = new GameObject("HealthBarContainer").transform;
			healthBarContainer.SetParent(uiRoot.transform);
		}

		private void LateUpdate()
		{
			foreach (var item in healthBars)
			{
				item.Move(mainCamera);
			}
		}

		public void AddHealthUI(ThinkingPlaceable p)
		{
			GameObject newUIObject = Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity,
				healthBarContainer);
			p.healthBar = newUIObject.GetComponent<HealthBar>();
			p.healthBar.Initialize(p);
			p.healthBar.Move(mainCamera);


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