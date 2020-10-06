using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace RoyaleBattle.Scripts.Timeline
{
	[Serializable, DisplayName("Card Marker")]
	public class CardMarker : Marker, INotification
	{
		public CardData card;
		public Vector3 position;
		public Placeable.Faction faction;

		public PropertyName id
		{
			get => new PropertyName();
		}
	}
}