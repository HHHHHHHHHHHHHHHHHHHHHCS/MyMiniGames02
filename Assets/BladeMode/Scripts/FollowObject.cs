using UnityEngine;

namespace BladeMode
{
    public class FollowObject : MonoBehaviour
    {
        public Transform target;
    
        private void Update()
        {
            transform.position = target.position;
        }
    }
}
