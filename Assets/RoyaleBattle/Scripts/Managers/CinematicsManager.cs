using UnityEngine;
using UnityEngine.Playables;

namespace RoyaleBattle
{
	public class CinematicsManager : MonoBehaviour
	{
		public PlayableDirector redCastleCollapse, blueCastleCollapse;

		public void PlayCollapseCutscene(Placeable.Faction f)
		{
			if (f == Placeable.Faction.Player)
			{
				redCastleCollapse.Play();
			}
			else
			{
				blueCastleCollapse.Play();
			}
		}
	}
}
