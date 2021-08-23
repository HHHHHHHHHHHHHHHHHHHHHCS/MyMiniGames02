using System;
using DG.Tweening;
using UnityEngine;

namespace KingdomHeartsShotLock.Scripts
{
	public class ShotLockTimeline : MonoBehaviour
	{
		[SerializeField] private ShotLock shotLock;
		[SerializeField] private KingdomHeartsShotLockMovementInput movementInput;

		private float playerY;

		private void Awake()
		{
		}

		private void OnEnable()
		{
			movementInput.enabled = false;
			playerY = movementInput.FloorY;
			shotLock.ActivateShotLock();
		}

		private void OnDisable()
		{
			shotLock.cinematic = false;
			shotLock.Aim(false);

			movementInput.transform.DOMoveY(playerY, 0.5f).SetEase(Ease.InSine)
				.OnComplete(() => { movementInput.enabled = true; });
		}
	}
}