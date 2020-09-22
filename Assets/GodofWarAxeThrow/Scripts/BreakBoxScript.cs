using UnityEngine;

namespace GodofWarAxeThrow.Scripts
{
	public class BreakBoxScript : MonoBehaviour
	{
		public GameObject breakedBox;

		public void Break()
		{
			GameObject breaked = Instantiate(breakedBox, transform.position, transform.rotation);
			Rigidbody[] rbs = breaked.GetComponentsInChildren<Rigidbody>();
			foreach (var rb in rbs)
			{
				rb.AddExplosionForce(150, transform.position, 30);
			}

			Destroy(gameObject);
		}
	}
}