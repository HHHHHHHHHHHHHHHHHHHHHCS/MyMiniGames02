using UnityEngine;

namespace BoTWStasis.Scripts
{
	public class AimUIAnimation : MonoBehaviour
	{
		public static AimUIAnimation instance;

		public CanvasGroup canvasGroup;
		public RectTransform animationSquare;
		public Sprite noTargetSprite;
		public Sprite targetSprite;
	}
}
