using System;
using UnityEngine;

namespace KingdomSelect
{
    //UI 跟随物体
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;

        private Camera mainCamera;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (target != null)
            {
                transform.position = mainCamera.WorldToScreenPoint(target.position);
            }
            
        }
    }
}
