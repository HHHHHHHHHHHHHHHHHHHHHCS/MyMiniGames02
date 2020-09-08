using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmashCSS
{
	public class CursorDetection : MonoBehaviour
	{
		public Transform currentCharacter;
		public Transform token;
		public Vector3 offset = new Vector3(-100, 100, 0);
		public bool hasToken;

		private Camera mainCamera;
		private GraphicRaycaster gr;
		private PointerEventData pointerEventData = new PointerEventData(null);


		private void Start()
		{
			mainCamera = Camera.main;

			gr = GetComponentInParent<GraphicRaycaster>();

			SmashCSSMono.instance.ShowCharacterInSlot(0, null);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				if (currentCharacter != null)
				{
					TokenFollow(false);
					SmashCSSMono.instance.ConfirmCharacter(0,
						SmashCSSMono.instance.characters[currentCharacter.GetSiblingIndex()]);
				}
			}

			if (Input.GetKeyDown(KeyCode.X))
			{
				SmashCSSMono.instance.confirmedCharacter = null;
				TokenFollow(true);
			}

			if (hasToken)
			{
				token.position = transform.position + offset;
			}

			pointerEventData.position = transform.position;//mainCamera.WorldToScreenPoint(transform.position);
			List<RaycastResult> results = new List<RaycastResult>();
			gr.Raycast(pointerEventData, results);

			if (hasToken)
			{
				if (results.Count > 0)
				{
					Transform raycastCharacter = results[0].gameObject.transform;

					if (raycastCharacter != currentCharacter)
					{
						if (currentCharacter != null)
						{
							var image = currentCharacter.Find("SelectedBorder").GetComponent<Image>();
							image.DOKill();
							image.color = Color.clear;
						}

						SetCurrentCharacter(raycastCharacter);
					}
				}
				else
				{
					if (currentCharacter != null)
					{
						var image = currentCharacter.Find("SelectedBorder").GetComponent<Image>();
						image.DOKill();
						image.color = Color.clear;
						SetCurrentCharacter(null);
					}
				}
			}
		}

		private void SetCurrentCharacter(Transform t)
		{
			if (t != null)
			{
				Image selectImage = t.Find("SelectedBorder").GetComponent<Image>();
				selectImage.color = Color.white;
				selectImage.DOColor(Color.red, 0.7f).SetLoops(-1);
			}

			currentCharacter = t;

			if (t != null)
			{
				int index = t.GetSiblingIndex();
				Character character = SmashCSSMono.instance.characters[index];
				SmashCSSMono.instance.ShowCharacterInSlot(0, character);
			}
			else
			{
				SmashCSSMono.instance.ShowCharacterInSlot(0, null);
			}
		}

		private void TokenFollow(bool trigger)
		{
			hasToken = trigger;
		}
	}
}