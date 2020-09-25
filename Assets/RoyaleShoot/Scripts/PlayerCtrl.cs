using UnityEngine;
namespace RoyaleShoot
{
    public class PlayerCtrl : MonoBehaviour
    {
        public float velocity = 6f;

        public Vector3 desiredMoveDirection;
        public bool blockRotationPlayer;
        public float desiredRotationSpeed = 0.1f;
        public float allowPlayerRotation = 0.1f;
        public bool isGround;
        
        private Animator anim;
        private Camera cam;
        private CharacterController controller;

	
        private float verticalVel;
        private Vector3 moveVector;
        
        private float inputX;
        private float inputZ;
        private float speed;


    }
}
