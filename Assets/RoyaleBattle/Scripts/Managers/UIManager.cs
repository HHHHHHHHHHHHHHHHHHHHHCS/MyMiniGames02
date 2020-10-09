using System.Collections.Generic;
using UnityEngine;

namespace RoyaleBattle
{
	public class UIManager : MonoBehaviour
	{
		public GameObject healthBarPrefab;
		public GameObject gameOverUI;

		private List<HealthBar> healthBars;
		private Transform healthBarContainer;
	}
}