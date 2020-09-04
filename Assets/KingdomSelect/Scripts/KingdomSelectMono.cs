using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomSelect
{
	[System.Serializable]
	public class Kingdom
	{
		public string name;

		[Range(-180, 180)] public float x;
		[Range(-89, 89)] public float y;

		[HideInInspector] public Transform visualPoint;
	}

	public class KingdomSelectMono : MonoBehaviour
	{
		public List<Kingdom> kingdoms = new List<Kingdom>();

		[Space] [Header("Public References")] public GameObject kingdomPointPrefab;
		public GameObject kingdomButtonPrefab;
		public Transform modelTransform;
		public Transform kingdomButtonsContainer;

		[Space] [Header("Tween Settings")] public float lookDuration;
		public Ease lookEase;

		[Space] public Vector2 visualOffset;

		private void Start()
		{
			foreach (var k in kingdoms)
			{
				SpawnKingdomPoint(k);
			}

			if (kingdoms.Count > 0)
			{
				LookAtKingdom(kingdoms[0]);
				EventSystem.current.SetSelectedGameObject(kingdomButtonsContainer.GetChild(0).gameObject);
			}
		}

		//生成黄色的点
		private void SpawnKingdomPoint(Kingdom k)
		{
			GameObject kingdom = Instantiate(kingdomPointPrefab, modelTransform);
			kingdom.transform.localEulerAngles = new Vector3(k.y + visualOffset.y, -k.x - visualOffset.x, 0);
			k.visualPoint = kingdom.transform.GetChild(0);

			SpawnKingdomButton(k);
		}

		//生成交互的UI 按钮
		private void SpawnKingdomButton(Kingdom k)
		{
			Kingdom kingdom = k;
			Button kingdomButton = Instantiate(kingdomButtonPrefab, kingdomButtonsContainer).GetComponent<Button>();
			kingdomButton.onClick.AddListener(() => LookAtKingdom(kingdom));

			kingdomButton.transform.GetChild(0).GetComponentInChildren<Text>().text = k.name;
		}

		//摄像机转向物体  UI转换坐标出现在物体上
		public void LookAtKingdom(Kingdom k)
		{
			Transform cameraParent = Camera.main.transform.parent;
			Transform cameraPivot = cameraParent.parent;

			cameraParent.DOLocalRotate(new Vector3(k.y, 0), lookDuration, RotateMode.Fast).SetEase(lookEase);
			cameraPivot.DOLocalRotate(new Vector3(0, -k.x, 0), lookDuration, RotateMode.Fast).SetEase(lookEase);

			FindObjectOfType<FollowTarget>().target = k.visualPoint;
		}
	}
}