using UnityEngine;
using UnityEngine.UI;

namespace BoTWArrow
{
    public class BowScript : MonoBehaviour
    {
        [Header("Bow")] public Transform bowModel;
        public Transform bowZoomTransform;
        private Vector3 bowOriginalPos, bowOriginalRot;

        [Space] [Header("Arrow")] public GameObject arrowPrefab;
        public Transform arrowSpawnOrigin;
        public Transform arrowModel;
        private Vector3 arrowOriginalPos;

        [Space] [Header("Parameters")] public Vector3 arrowImpulse;
        public float timeToShoot;
        public float shootWait;
        public bool canShoot;
        public bool shootRest;
        public bool imAiming;

        [Space] public float zoomInDuration;
        public float zoomOutDuration;


        private float camOriginFov;
        public float camZoomFov;
        private Vector3 camOriginalPos;
        public Vector3 camZoomOffset;

        [Space] [Header("Particles")] public ParticleSystem[] prepareParticles;
        public ParticleSystem[] aimParticles;
        public GameObject circleParticlePrefab;

        [Space] [Header("Canvas")] public RectTransform reticle;
        public CanvasGroup reticleCanvas;
        public Image centerCircle;
        public Vector2 originalImage;


    }
}
