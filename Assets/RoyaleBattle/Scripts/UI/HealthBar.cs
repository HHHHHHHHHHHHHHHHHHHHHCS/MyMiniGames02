using UnityEngine;
using UnityEngine.UI;

namespace RoyaleBattle
{
	public class HealthBar : MonoBehaviour
	{
		public RectTransform bar;
		public GameObject wholeWidget;

		private bool isHidden = true;
		private float originHP;
		private float currentHP;
		private Transform transformToFollow;

		private Color red = new Color32(252, 35, 13, 255);
		private Color blue = new Color32(31, 132, 255, 255);

		public void Initialize(ThinkingPlaceable p)
		{
			originHP = currentHP = p.hitPoints;

			transformToFollow = p.transform;

			bar.GetComponent<Image>().color = (p.faction == Placeable.Faction.Player) ? red : blue;

			wholeWidget.transform.localPosition = new Vector3(0f,
				(p.pType == Placeable.PlaceableType.Unit) ? 3f : 6f,
				(p.pType == Placeable.PlaceableType.Unit) ? 0f : -2f);

			wholeWidget.SetActive(false);
		}

		public void SetHealth(float newHP)
		{
			if (isHidden)
			{
				wholeWidget.SetActive(true);
				isHidden = false;
			}

			float ratio = 0f;
			if (newHP > 0f)
			{
				ratio = newHP / originHP;
			}

			bar.localScale = new Vector3(ratio, 1f, 1f);
		}

		public void Move()
		{
			if (transformToFollow != null)
			{
				transform.position = transformToFollow.position;
			}
		}
	}
}