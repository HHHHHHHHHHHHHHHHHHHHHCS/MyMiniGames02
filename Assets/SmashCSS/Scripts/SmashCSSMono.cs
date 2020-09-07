using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SmashCSS
{
	public class SmashCSSMono : MonoBehaviour
	{
		public static SmashCSSMono instance;

		[HideInInspector] public Vector2 slotArtworkSize;
		[Header("Characters List")] public List<Character> characters = new List<Character>();

		[Space] [Header("Public References")] public GameObject charCellPrefab;
		public GameObject gridBgPrefab;
		public Transform playerSlotsContainer;

		[Space] [Header("Current Confirmed Character")]
		public Character confirmedCharacter;

		private GridLayoutGroup gridLayout;

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			GetComponent<RectTransform>().sizeDelta = new Vector2(gridLayout.cellSize.x * 5, gridLayout.cellSize.y * 2);
			RectTransform gridBG = Instantiate(gridBgPrefab, transform.parent).GetComponent<RectTransform>();
			gridBG.transform.SetSiblingIndex(transform.GetSiblingIndex());
			gridBG.sizeDelta = GetComponent<RectTransform>().sizeDelta;

			slotArtworkSize = playerSlotsContainer.GetChild(0).Find("Artwork").GetComponent<RectTransform>().sizeDelta;

			foreach (var character in characters)
			{
				SpawnCharacterCell(character);
			}
		}

		private void SpawnCharacterCell(Character character)
		{
			GameObject charCell = Instantiate(charCellPrefab, transform);

			charCell.name = character.characterName;

			Image artwork = charCell.transform.Find("Artwork").GetComponent<Image>();
			Text nameText = charCell.transform.Find("NameRect/NameText").GetComponent<Text>();

			artwork.sprite = character.characterSprite;
			nameText.text = character.characterName;

			RectTransform trans = artwork.GetComponent<RectTransform>();
			trans.pivot = UIPivot(artwork.sprite);
			trans.sizeDelta *= character.zoom;
		}

		private void ShowCharacterInSlot(int player, Character character)
		{
			bool nullChar = (character == null);

			Color alpha = nullChar ? Color.clear : Color.white;
			Sprite artwork = nullChar ? null : character.characterSprite;
			string name = nullChar ? string.Empty : character.characterName;
			string playerNickName = "Player " + (player + 1);
			string playerNumber = "P" + (player + 1);

			Transform slot = playerSlotsContainer.GetChild(player);

			Transform slotArtwork = slot.Find("Artwork");
			Transform slotIcon = slot.Find("Icon");

			Sequence s = DOTween.Sequence();
			s.Append(slotArtwork.DOLocalMoveX(-300, 0.05f).SetEase(Ease.OutCubic));
			s.AppendCallback(() =>
			{
				var image = slotArtwork.GetComponent<Image>();
				image.sprite = artwork;
				image.color = alpha;
			});
			s.Append(slotArtwork.DOLocalMoveX(300, 0f));
			s.Append(slotArtwork.DOLocalMoveX(0, 0.05f).SetEase(Ease.OutCubic));

			Image slotIconImage = slotIcon.GetComponent<Image>();
			if (nullChar)
			{
				slotIconImage.DOFade(0, 0);
			}
			else
			{
				slotIconImage.sprite = character.characterIcon;
				slotIconImage.DOFade(0.3f, 0f);
			}

			if (artwork != null)
			{
				RectTransform slotArtworkTs = slotArtwork.GetComponent<RectTransform>();
				slotArtworkTs.pivot = UIPivot(artwork);
				slotArtworkTs.sizeDelta = slotArtworkSize * character.zoom;
			}

			slot.Find("Name").GetComponent<Text>().text = name;
			slot.Find("Player").GetComponentInChildren<Text>().text = playerNickName;
			slot.Find("IconAndPx").GetComponentInChildren<Text>().text = playerNumber;
		}

		public void ConfirmCharacter(int player, Character character)
		{
			if (confirmedCharacter == null)
			{
				confirmedCharacter = character;
				playerSlotsContainer.GetChild(player).DOPunchPosition(Vector3.down * 3, 0.3f, 10, 1f);
			}
		}


		/// <summary>
		/// 使用UI原来的 Pivot
		/// </summary>
		/// <param name="sprite"></param>
		/// <returns></returns>
		private Vector2 UIPivot(Sprite sprite)
		{
			Vector2 pixelSize = new Vector2(sprite.texture.width, sprite.texture.height);
			Vector2 pixelPivot = sprite.pivot;
			return new Vector2(pixelPivot.x / pixelSize.x, pixelPivot.y / pixelSize.y);
		}
	}
}