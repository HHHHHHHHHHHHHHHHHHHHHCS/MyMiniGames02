using UnityEngine;

namespace RoyaleBattle
{
	
	[CreateAssetMenu(fileName = "NewCard" , menuName = "Unity Royale/Card Data")]
	public class CardData : ScriptableObject
	{
		[Header("Card graphics")]
		public Sprite cardImage;

		[Header("List of Placeables")]
		public PlaceableData[] placeablesData;
		public Vector3[] relativeOffsets;
	}
}
