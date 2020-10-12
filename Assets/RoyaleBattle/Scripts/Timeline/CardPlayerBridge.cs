using UnityEngine;
using UnityEngine.Playables;

namespace RoyaleBattle
{
	public class CardPlayerBridge : MonoBehaviour, INotificationReceiver
	{
		public GameManager gameManager;

		public void OnNotify(Playable origin, INotification notification, object context)
		{
			CardMarker cm = notification as CardMarker;

			if (cm != null)
			{
				gameManager.UseCard(cm.card, cm.position, cm.faction);
			}
		}
	}
}