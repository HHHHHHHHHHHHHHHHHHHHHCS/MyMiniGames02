using UnityEngine;

namespace RoyaleBattle
{
	[CreateAssetMenu(fileName = "NewPlaceable", menuName = "Unity Royale/Placceable Data")]
	public class PlaceableData : ScriptableObject
	{
		[Header("Common")] public Placeable.PlaceableType pType;
		public GameObject associatedPrefab;
		public GameObject alternatePrefab;

		[Header("Units and Buildings")] public ThinkingPlaceable.AttackType attackType;
		public Placeable.PlaceableTarget targetType = Placeable.PlaceableTarget.Both;
		public float attackRatio = 1f;
		public float damagePerAttack = 2f;
		public float attackRange = 1f;
		public float hitPoints = 10f;
		public AudioClip attackClip;
		public AudioClip dieClip;

		[Header("Units")] public float speed = 5f;

		[Header("Obstacles and Spells")] public float lifeTime = 5f;

		[Header("Spells")] public float damagePerSecond = 1f;
	}
}