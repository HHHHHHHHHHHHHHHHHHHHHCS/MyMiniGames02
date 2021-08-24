using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarioKartDrift.Scripts
{
	public class KartController : MonoBehaviour
	{
		public Volume volume;
		private VolumeProfile volumeProfile;

		public Transform kartModel;
		public Transform kartNormal;
		public Rigidbody sphere;

		public List<ParticleSystem> primaryParticles = new List<ParticleSystem>();
		public List<ParticleSystem> secondaryParticles = new List<ParticleSystem>();

		private float speed, currentSpeed;
		private float rotate, currentRotate;
		private int driftDirection;
		private float driftPower;
		private int driftMode = 0;
		private bool first, second, third;
		private Color c;

		[Header("Bools")] public bool drifting;

		[Header("Parameters")] public float acceleration = 30f;
		public float steering = 80f;
		public float gravity = 10f;
		public LayerMask layerMask;

		[Header("Model Parts")] public Transform frontWheels;
		public Transform backWheels;
		public Transform steeringWheel;

		[Header("Particles")] public Transform wheelParticles;
		public Transform flashParticles;
		public Color[] turboColors;

		private void Start()
		{
			volumeProfile = volume.profile;

			for (int i = 0; i < wheelParticles.GetChild(0).childCount; i++)
			{
				primaryParticles.Add(wheelParticles.GetChild(0).GetChild(i).GetComponent<ParticleSystem>());
			}

			for (int i = 0; i < wheelParticles.GetChild(1).childCount; i++)
			{
				primaryParticles.Add(wheelParticles.GetChild(1).GetChild(i).GetComponent<ParticleSystem>());
			}

			foreach (ParticleSystem p in flashParticles.GetComponentsInChildren<ParticleSystem>())
			{
				secondaryParticles.Add(p);
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				float time = Time.timeScale == 1 ? 0.2f : 1f;
				Time.timeScale = time;
			}

			
			
			//Follow Collider 
			transform.position = sphere.transform.position - new Vector3(0f, 0.4f, 0f);

			//Accelerate
			if (Input.GetButton("Fire1"))
			{
				speed = acceleration;
			}

			//Steer
			if (Input.GetAxis("Horizontal") != 0)
			{
				int dir = (int) Input.GetAxisRaw("Horizontal");
				float amount = Mathf.Abs(Input.GetAxis("Horizontal"));
				Steer(dir, amount);
			}

			//Drift
			if (Input.GetButtonDown("Jump") && !drifting && Input.GetAxis("Horizontal") != 0)
			{
				drifting = true;
				driftDirection = (int) Input.GetAxisRaw("Horizontal");

				foreach (ParticleSystem p in primaryParticles)
				{
					var m = p.main;
					m.startColor = Color.clear;
					p.Play();
				}

				kartModel.parent.DOComplete();
				kartModel.parent.DOPunchPosition(transform.up * 0.2f, 0.3f, 5, 1f);
			}

			if (drifting)
			{
				float from2, to2;
				if (driftDirection == 1)
				{
					from2 = 0;
					to2 = 2;
				}
				else
				{
					from2 = 2;
					to2 = 0;
				}

				float control = math.remap(Input.GetAxis("Horizontal"), -1, 1, from2, to2);

				if (driftDirection == 1)
				{
					from2 = 0.2f;
					to2 = 1;
				}
				else
				{
					from2 = 1f;
					to2 = 0.2f;
				}

				float powerControl = math.remap(Input.GetAxis("Horizontal"), -1, 1, from2, to2);
				Steer(driftDirection, control);
				driftPower += powerControl;
				ColorDrift();
			}

			if (Input.GetButtonUp("Jump") && drifting)
			{
				Boost();
			}

			currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 0.12f);
			speed = 0f;
			currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
			rotate = 0f;
			
			//Animations    

			//a) Kart
			if (!drifting)
			{
				
			}
		}
	}
}