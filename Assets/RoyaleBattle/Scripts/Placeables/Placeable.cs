using UnityEngine;
using UnityEngine.Events;

namespace RoyaleBattle
{
	public class Placeable : MonoBehaviour
	{
		public PlaceableType pType;

		[HideInInspector] public Faction faction;
		[HideInInspector] public PlaceableTarget target; //TODO:移动到ThinkingPlaceable?
		[HideInInspector] public AudioClip dieAudioClip;

		public UnityAction<Placeable> OnDie;

		//单位的类型
		public enum PlaceableType
		{
			Unit,
			Obstacle,
			Building,
			Spell,
			Castle, //特殊的建筑类型
		}

		//攻击的目标类型
		public enum PlaceableTarget
		{
			OnlyBuildings,
			Both,
			None,
		}

		//所属类型
		public enum Faction
		{
			Player, //Red
			Opponent, //Blue
			None,
		}
	}
}