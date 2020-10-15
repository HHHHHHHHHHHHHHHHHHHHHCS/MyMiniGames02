using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
			//初始化基地
			SetupPlaceable(playersCastle, castlePData, Placeable.Faction.Player);
			SetupPlaceable(opponentCastle, castlePData, Placeable.Faction.Opponent);

			cardManager.LoadDeck();
			cpuOpponent.LoadDeck();

			//audioManager.GoToDefaultSnapshot();

			if (autoStart)
			{
				StartMatch();
			}
		}

		public void StartMatch()
		{
			cpuOpponent.StartActing();
		}

		private void Update()
		{
			if (gameover)
			{
				return;
			}
			
			ThinkingPlaceable targetToPass; //ref
			ThinkingPlaceable p; //ref

			for (int pN = 0; pN < allThinkingPlaceables.Count; pN++)
			{
				if (gameover)
				{
					return;
				}
				
				p = allThinkingPlaceables[pN];

				if (updateAllPlaceables)
				{
					p.state = ThinkingPlaceable.States.Idle;
				}

				switch (p.state)
				{
					case ThinkingPlaceable.States.Idle:
					{
						if (p.targetType == Placeable.PlaceableTarget.None)
						{
							break;
						}

						bool targetFound = FindClosesInList(p.transform.position,
							GetAttackList(p.faction, p.targetType), out targetToPass);
						if (!targetFound)
						{
							//this should only happen on Game Over
							Debug.LogError("No more targets!");
						}
						
						p.SetTarget(targetToPass);
						p.Seek();
						break;
					}
					case ThinkingPlaceable.States.Seeking:
					{
						if (p.IsTargetInRange())
						{
							p.StartAttack();
						}
						break;
					}
					case ThinkingPlaceable.States.Attacking:
					{
						if (p.IsTargetInRange())
						{
							if (Time.time >= p.lastBlowTime + p.attackRatio)
							{
								p.DealBlow();
							}
						}
						break;
					}
					case ThinkingPlaceable.States.Dead:
					{
						Debug.LogError("A dead ThinkingPlaceable shouldn't be in this loop");
						break;
					}
				}

				Projectile currProjectile;
				float progressToTarget;
				for (int prjN = 0; prjN < allProjectiles.Count; prjN++)
				{
					currProjectile = allProjectiles[prjN];
					progressToTarget = currProjectile.Move();
					if (progressToTarget >= 1f)
					{
						if (currProjectile.target.state != ThinkingPlaceable.States.Dead)
						{
							float newHP = currProjectile.target.SufferDamage(currProjectile.damage);
							currProjectile.target.healthBar.SetHealth(newHP);
						}
						Destroy(currProjectile.gameObject);
						allProjectiles.RemoveAt(prjN);
					}
				}
			}
			
			updateAllPlaceables = false;//usecard set true
		}

		private List<ThinkingPlaceable> GetAttackList(Placeable.Faction f, Placeable.PlaceableTarget t)
		{
			switch (t)
			{
				case Placeable.PlaceableTarget.Both:
					return (f == Placeable.Faction.Player) ? allOpponents : allPlayers;
				case Placeable.PlaceableTarget.OnlyBuildings:
					return (f == Placeable.Faction.Player) ? opponentBuildings : playerBuildings;
				default:
					Debug.LogError("What faction is this?? Not Player nor Opponent");
					return null;
			}
		}

		private bool FindClosesInList(Vector3 p, List<ThinkingPlaceable> list, out ThinkingPlaceable t)
		{
			t = null;
			bool targetFound = false;
			float closestDistanceSqr = Mathf.Infinity;

			for (int i = 0; i < list.Count; i++)
			{
				float sqrDistance = (p - list[i].transform.position).sqrMagnitude;
				if (sqrDistance < closestDistanceSqr)
				{
					t = list[i];
					closestDistanceSqr = sqrDistance;
					targetFound = true;
				}
			}

			return targetFound;
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
					Unit uScript = go.GetComponent<Unit>();
					uScript.Activate(pFaction, pDataRef);
					uScript.OnDealDamage += OnPlaceableDealtDamage;
					uScript.OnProjectileFired += OnProjectileFired;
					AddPlaceableToList(uScript);
					uiManager.AddHealthUI(uScript);
					break;
				}
				case Placeable.PlaceableType.Building:
				case Placeable.PlaceableType.Castle:
				{
					Building bScript = go.GetComponent<Building>();
					bScript.Activate(pFaction, pDataRef);
					bScript.OnDealDamage += OnPlaceableDealtDamage;
					bScript.OnProjectileFired += OnProjectileFired;
					AddPlaceableToList(bScript);
					uiManager.AddHealthUI(bScript);

					if (pDataRef.pType == Placeable.PlaceableType.Castle)
					{
						bScript.OnDie += OnCastleDead;
					}

					navMesh.BuildNavMesh(); //rebake the navmesh
					break;
				}
				case Placeable.PlaceableType.Obstacle:
				{
					Obstacle oScript = go.GetComponent<Obstacle>();
					oScript.Activate(pDataRef);
					navMesh.BuildNavMesh(); //rebake the navmesh
					break;
				}
				case Placeable.PlaceableType.Spell:
				{
					//TODO:spell
					break;
				}
			}

			go.GetComponent<Placeable>().OnDie += OnPlaceableDead;
		}


		private void OnPlaceableDealtDamage(ThinkingPlaceable p)
		{
			if (p.target.state != ThinkingPlaceable.States.Dead)
			{
				float newHealth = p.target.SufferDamage(p.damage);
				p.target.healthBar.SetHealth(newHealth);
			}
		}

		private void OnProjectileFired(ThinkingPlaceable p)
		{
			Vector3 adjTargetPos = p.target.transform.position;
			adjTargetPos.y = 1.5f;
			Quaternion rot = Quaternion.LookRotation(adjTargetPos - p.projectileSpawnPoint.position);
			Projectile proj = GameObject
				.Instantiate<GameObject>(p.projectilePrefab, p.projectileSpawnPoint.position, rot)
				.GetComponent<Projectile>();
			proj.target = p.target;
			proj.damage = p.damage;
			allProjectiles.Add(proj);
		}

		private void OnCastleDead(Placeable c)
		{
			cinematicsManager.PlayCollapseCutscene(c.faction);
			c.OnDie -= OnCastleDead;
			gameover = true; //stop and thinking loop
			
			ThinkingPlaceable thkPl;
			for (int pN = 0; pN < allThinkingPlaceables.Count; pN++)
			{
				thkPl = allThinkingPlaceables[pN];
				if (thkPl.state != ThinkingPlaceable.States.Dead)
				{
					thkPl.Stop();
					thkPl.transform.LookAt(c.transform.position);
					uiManager.RemoveHealthUI(thkPl);
				}
			}

			//audioManager.GoToEndMatchSnapshot();
			cpuOpponent.StopActing();
		}

		private void OnPlaceableDead(Placeable p)
		{
			p.OnDie -= OnPlaceableDead;

			switch (p.pType)
			{
				case Placeable.PlaceableType.Unit:
				{
					Unit u = p as Unit;
					RemovePlaceableFromList(u);
					u.OnDealDamage -= OnPlaceableDealtDamage;
					u.OnProjectileFired -= OnProjectileFired;
					uiManager.RemoveHealthUI(u);
					StartCoroutine(Dispose(u));
					break;
				}
				case Placeable.PlaceableType.Building:
				case Placeable.PlaceableType.Castle:
				{
					Building b = p as Building;
					RemovePlaceableFromList(b);
					uiManager.RemoveHealthUI(b);
					b.OnDealDamage -= OnPlaceableDealtDamage;
					b.OnProjectileFired -= OnProjectileFired;
					StartCoroutine(RebuildNavmesh());

					if (p.pType != Placeable.PlaceableType.Castle)
					{
						StartCoroutine(Dispose(b));
					}

					break;
				}
				case Placeable.PlaceableType.Obstacle:
				{
					StartCoroutine(RebuildNavmesh());
					break;
				}
				case Placeable.PlaceableType.Spell:
				{
					//TODO: can spells die?
					break;
				}
			}
		}

		public void OnEndGameCutsceneOver()
		{
			uiManager.ShowGameOverUI();
		}

		private IEnumerator Dispose(ThinkingPlaceable p)
		{
			yield return new WaitForSeconds(3f);

			Destroy(p.gameObject);
		}

		private IEnumerator RebuildNavmesh()
		{
			yield return new WaitForEndOfFrame();

			navMesh.BuildNavMesh();
		}

		private void AddPlaceableToList(ThinkingPlaceable p)
		{
			allThinkingPlaceables.Add(p);

			if (p.faction == Placeable.Faction.Player)
			{
				allPlayers.Add(p);

				if (p.pType == Placeable.PlaceableType.Unit)
				{
					playerUnits.Add(p);
				}
				else
				{
					playerBuildings.Add(p);
				}
			}
			else if (p.faction == Placeable.Faction.Opponent)
			{
				allOpponents.Add(p);

				if (p.pType == Placeable.PlaceableType.Unit)
				{
					opponentUnits.Add(p);
				}
				else
				{
					opponentBuildings.Add(p);
				}
			}
			else
			{
				Debug.LogError("Error in adding a Placeable in one of the player/opponent lists");
			}
		}

		private void RemovePlaceableFromList(ThinkingPlaceable p)
		{
			allThinkingPlaceables.Remove(p);

			if (p.faction == Placeable.Faction.Player)
			{
				allPlayers.Remove(p);

				if (p.pType == Placeable.PlaceableType.Unit)
				{
					playerUnits.Remove(p);
				}
				else
				{
					playerBuildings.Remove(p);
				}
			}
			else if (p.faction == Placeable.Faction.Opponent)
			{
				allOpponents.Remove(p);

				if (p.pType == Placeable.PlaceableType.Unit)
				{
					opponentUnits.Remove(p);
				}
				else
				{
					opponentBuildings.Remove(p);
				}
			}
			else
			{
				Debug.LogError("Error in removing a Placeable from one of the player/opponent lists");
			}
		}
	}
}