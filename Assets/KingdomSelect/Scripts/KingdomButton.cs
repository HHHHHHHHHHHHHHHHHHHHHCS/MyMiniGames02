using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomSelect
{
	public class KingdomButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler,
		IPointerExitHandler
	{
		public Text text;
		public Image rect;
		public Image circle;

		public Color textColorWhenSelected;
		public Color rectColorMouseOver;

		private void Start()
		{
			rect.color = Color.clear;
			text.color = Color.white;
			circle.color = Color.white;
		}

		public void OnSelect(BaseEventData eventData)
		{
			rect.DOColor(Color.white, 0.1f);
			text.DOColor(textColorWhenSelected, 0.1f);
			circle.DOColor(Color.red, 0.1f);

			rect.transform.DOComplete();
			//在 0.2 秒内在原始比例和下面比例之间，来回冲压变化
			rect.transform.DOPunchScale(Vector3.one / 3, 0.2f, 20, 1);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			rect.DOColor(Color.clear, .1f);
			text.DOColor(Color.white, .1f);
			circle.DOColor(Color.white, .1f);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (EventSystem.current.currentSelectedGameObject != gameObject)
			{
				rect.DOColor(rectColorMouseOver, .2f);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (EventSystem.current.currentSelectedGameObject != gameObject)
			{
				rect.DOColor(Color.clear, .2f);
			}
		}
	}
}