using UnityEngine;

namespace FFXVWarpStrike
{
	public class SkillTarget : MonoBehaviour
	{
		private WeaponCtrl warp;

		void Start()
		{
			warp = WeaponCtrl.Instance;
		}

		private void OnBecameVisible()
		{
			if (warp)
			{
				warp.AddSkillTarget(this);
			}
		}

		private void OnBecameInvisible()
		{
			if (warp)
			{
				warp.RemoveSkillTarget(this);
			}
		}
	}
}