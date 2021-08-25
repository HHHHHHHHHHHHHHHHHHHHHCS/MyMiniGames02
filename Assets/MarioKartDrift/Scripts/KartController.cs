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

			float hor = Input.GetAxis("Horizontal");
			int horRaw = (int) Input.GetAxisRaw("Horizontal");
			// float ver = Input.GetAxis("Vertical");
			// int verRaw = (int) Input.GetAxisRaw("Vertical");
			bool isJumpDown = Input.GetButtonDown("Jump");
			bool isJumpUp = Input.GetButtonUp("Jump");
			bool isAcc = Input.GetButton("Fire1");

			//Follow Collider 
			transform.position = sphere.transform.position - new Vector3(0f, 0.4f, 0f);

			//Accelerate
			if (isAcc)
			{
				speed = acceleration;
			}

			//Steer
			if (hor != 0)
			{
				int dir = horRaw;
				float amount = Mathf.Abs(hor);
				Steer(dir, amount);
			}

			//Drift
			if (isJumpDown && !drifting && hor != 0)
			{
				drifting = true;
				driftDirection = horRaw;

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

				float control = math.remap(hor, -1, 1, from2, to2);

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

				float powerControl = math.remap(hor, -1, 1, from2, to2);
				Steer(driftDirection, control);
				driftPower += powerControl;
				ColorDrift();
			}

			if (isJumpUp && drifting)
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
				kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles,
					new Vector3(0, 90 + (hor * 15), kartModel.localEulerAngles.z), 0.2f);
			}
			else
			{
				float from2, to2;
				if (driftDirection == 1)
				{
					from2 = 0.5f;
					to2 = 2f;
				}
				else
				{
					from2 = 2f;
					to2 = 0.5f;
				}

				float control = math.remap(hor, -1, 1, from2, to2);
				kartModel.parent.localRotation = quaternion.Euler(0,
					Mathf.LerpAngle(kartModel.parent.localEulerAngles.y, (control * 15) * driftDirection, 0.2f), 0f);
			}

			//b) Wheels
			frontWheels.localEulerAngles = new Vector3(0, hor * 15f, frontWheels.localEulerAngles.z);
			frontWheels.localEulerAngles += new Vector3(0f, 0f, sphere.velocity.magnitude / 2f);
			backWheels.localEulerAngles += new Vector3(0f, 0f, sphere.velocity.magnitude / 2f);

			//c) Steering Wheel
			steeringWheel.localEulerAngles = new Vector3(-25f, 90, hor * 45);
		}

		private void FixedUpdate()
		{
			//Forward Acceleration
			if (!drifting)
			{
				sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);
			}
			else
			{
				sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
			}

			//Gravity
			sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

			//Steering
			transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
				new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);

			RaycastHit hitOn, hitNear;

			Physics.Raycast(transform.position + (transform.up * 0.1f), Vector3.down, out hitOn, 1.1f, layerMask);
			Physics.Raycast(transform.position + (transform.up * 0.1f), Vector3.down, out hitNear, 2.0f, layerMask);

			//Normal Rotation
			kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
			kartNormal.Rotate(0, transform.eulerAngles.y, 0);
		}
	}
}