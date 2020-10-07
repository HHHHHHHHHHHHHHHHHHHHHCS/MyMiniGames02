using UnityEngine;

namespace SmashCSS
{
	[CreateAssetMenu(fileName = "New Character", menuName = "SmashCSS/Character")]
	[System.Serializable]
	public class Character : ScriptableObject
	{
		public string characterName;
		public Sprite characterSprite;
		public Sprite characterIcon;
		public float zoom = 1;
	}
}