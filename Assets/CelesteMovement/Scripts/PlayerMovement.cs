using System;
using UnityEngine;

namespace CelesteMovement.Scripts
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Stats")] public float spped = 10;
        public float jumpForce = 50;
        public float slideSpeed = 5;
        public float wallJumpLerp = 10;
        public float dashSpeed = 20;

        [Space, Header("Booleans")] public bool canMove;
        public bool wallGrab;
        public bool wallJumped;
        public bool wallSlide;
        public bool isDashing;

        public int side = 1;


        [Space, Header("Polish")] public ParticleSystem dashParticle;
        public ParticleSystem jumpParticle;
        public ParticleSystem wallJumpParticle;
        public ParticleSystem slideParticle;
        
        
        private bool groundTouch;
        private bool hasDashed;

        private PlayerAnimation anim;
        private PlayerCollision collision;
        private Rigidbody2D rigi;

        private void Start()
        {
            collision = GetComponent<PlayerCollision>();
            rigi = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<PlayerAnimation>();
        }
    }
}
