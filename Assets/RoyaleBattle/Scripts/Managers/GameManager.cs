using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RoyaleBattle
{
	public class GameManager : MonoBehaviour
	{
		private const float THINKING_DELAY = 2f;

		[Header("Setting")] public bool autoStart;

		[Header("Public References")] public NavMeshSurface navMesh;
		public GameObject playersCastle, opponentCastle;
		public GameObject introTimeline;
		public PlaceableData castlePData;
		public ParticlePool appearEffectPool;

		private CardManager cardManager;
		private CPUOpponent cpuOpponent;
		private InputManager inputManager;
		private AudioManager audioManager;
		private UIManager uiManager;
		private CinematicsManager cinematicsManager;

		private List<ThinkingPlaceable> playerUnits, opponentUnits;
		private List<ThinkingPlaceable> playerBuildings, opponentBuildings;
		private List<ThinkingPlaceable> allPlayers, allOpponents; //这个容器包括单位和建筑
		private List<ThinkingPlaceable> allThinkingPlaceables;
		private List<Projectile> allProjectiles;
		private bool gameover = false;
		private bool updateAllPlaceables = false; //更新AI下次的update loop

		private void Awake()
		{
			cardManager = GetComponent<CardManager>();
			cpuOpponent = GetComponent<CPUOpponent>();
			inputManager = GetComponent<InputManager>();
			//audioManager = GetComponentInChildren<AudioManager>();
			cinematicsManager = GetComponentInChildren<CinematicsManager>();
			uiManager = GetComponent<UIManager>();

			if (autoStart)
			{
				introTimeline.SetActive(false);
			}

			cardManager.OnCardUsed += UseCard;
			cpuOpponent.OnCardUsed += UseCard;

			playerUnits = new List<ThinkingPlaceable>();
			playerBuildings = new List<ThinkingPlaceable>();
			opponentUnits = new List<ThinkingPlaceable>();
			opponentBuildings = new List<ThinkingPlaceable>();
			allPlayers = new List<ThinkingPlaceable>();
			allOpponents = new List<ThinkingPlaceable>();
			allThinkingPlaceables = new List<ThinkingPlaceable>();
			allProjectiles = new List<Projectile>();
		}

		private void Start()
		{
			SetupPlaceable(playersCastle, castlePData, Placeable.Faction.Player);
			SetupPlaceable(opponentCastle, castlePData, Placeable.Faction.Opponent);
		}


		public void UseCard(CardData cardData, Vector3 position, Placeable.Faction pFaction)
		{
			for (int pNum = 0; pNum < cardData.placeablesData.Length; pNum++)
			{
				PlaceableData pDataRef = cardData.placeablesData[pNum];

				Quaternion rot = (pFaction == Placeable.Faction.Player)
					? Quaternion.identity
					: Quaternion.Euler(0f, 180f, 0f);
				GameObject prefabToSpawn = (pFaction == Placeable.Faction.Player)
					? pDataRef.associatedPrefab
					: ((pDataRef.alternatePrefab == null) ? pDataRef.associatedPrefab : pDataRef.alternatePrefab);
				GameObject newPlaceableGO =
					GameObject.Instantiate<GameObject>(prefabToSpawn, position + cardData.relativeOffsets[pNum], rot);

				SetupPlaceable(newPlaceableGO, pDataRef, pFaction);

				appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
			}

			updateAllPlaceables = true; //更新AI下次的update loop
		}


		private void SetupPlaceable(GameObject go, PlaceableData pDataRef, Placeable.Faction pFaction)
		{
			switch (pDataRef.pType)
			{
				case Placeable.PlaceableType.Unit:
				{
					unit 
					break;
				}
				case Placeable.PlaceableType.Obstacle:
					break;
				case Placeable.PlaceableType.Building:
					break;
				case Placeable.PlaceableType.Spell:
					break;
				case Placeable.PlaceableType.Castle:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
	}
}