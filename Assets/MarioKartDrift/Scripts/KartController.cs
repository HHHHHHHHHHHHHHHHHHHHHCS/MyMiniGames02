using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MarioKartDrift.Scripts
{
	//其实应该假如Animator 可以摆动一下
	public class KartController : MonoBehaviour
	{
		public Volume volume;

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

		private ChromaticAberration chromaticAberration;

		private ParticleSystem tube001PS, tube002PS;
		private CinemachineImpulseSource cameraImpulseSource;

		private void Start()
		{
			volume.profile.TryGet(out chromaticAberration);

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

			tube001PS = kartModel.Find("Tube001").GetComponentInChildren<ParticleSystem>();
			tube002PS = kartModel.Find("Tube002").GetComponentInChildren<ParticleSystem>();

			cameraImpulseSource = GameObject.Find("CM vcam1").GetComponent<CinemachineImpulseSource>();
		}

		private void Update()
		{


			float hor = Input.GetAxis("Horizontal");
			int horRaw = (int) Input.GetAxisRaw("Horizontal");
			// float ver = Input.GetAxis("Vertical");
			// int verRaw = (int) Input.GetAxisRaw("Vertical");
			bool isJumpDown = Input.GetButtonDown("Jump");
			bool isJumpUp = Input.GetButtonUp("Jump");
			bool isAcc = Input.GetAxis("Vertical") > 0; // Input.GetButton("Fire1");

			if (isJumpDown)
			{
				Time.timeScale = 0.2f;
			}
			
			if (isJumpUp)
			{
				Time.timeScale = 1f;
			}
			
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

				float control = math.remap(-1, 1, from2, to2, hor);

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

				float powerControl = math.remap(-1, 1, from2, to2, hor);
				Steer(driftDirection, control);
				driftPower += powerControl;
				ColorDrift();
			}

			if (isJumpUp && drifting)
			{
				Boost();
			}

			currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
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

				float control = math.remap(-1, 1, from2, to2, hor);
				kartModel.parent.localRotation = Quaternion.Euler(0,
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

		public void Boost()
		{
			drifting = false;

			if (driftMode > 0)
			{
				DOVirtual.Float(currentSpeed * 3.0f, currentSpeed, 0.3f * driftMode, Speed);
				DOVirtual.Float(0.0f, 1.0f, 0.5f, ChromaticAmount)
					.OnComplete(() => DOVirtual.Float(1.0f, 0.0f, 0.5f, ChromaticAmount));
				tube001PS.Play();
				tube002PS.Play();
			}

			driftPower = 0;
			driftMode = 0;
			first = false;
			second = false;
			third = false;

			foreach (ParticleSystem p in primaryParticles)
			{
				var mainModule = p.main;
				mainModule.startColor = Color.clear;
				p.Stop();
			}

			kartModel.parent.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
		}

		public void Steer(int direction, float amount)
		{
			rotate = (steering * direction) * amount;
		}

		public void ColorDrift()
		{
			if (!first)
			{
				c = Color.clear;
			}

			if (driftPower > 50f && driftPower < 100 - 1 && !first)
			{
				first = true;
				c = turboColors[0];
				driftMode = 1;

				PlayFlashParticle(c);
			}

			if (driftPower > 100 && driftPower < 150 - 1 && !second)
			{
				second = true;
				c = turboColors[1];
				driftMode = 2;

				PlayFlashParticle(c);
			}

			if (driftPower > 150 && !third)
			{
				third = true;
				c = turboColors[2];
				driftMode = 3;

				PlayFlashParticle(c);
			}

			foreach (ParticleSystem particle in primaryParticles)
			{
				var mainModule = particle.main;
				mainModule.startColor = c;
			}

			foreach (ParticleSystem particle in secondaryParticles)
			{
				var mainModule = particle.main;
				mainModule.startColor = c;
			}
		}

		private void PlayFlashParticle(Color c)
		{
			cameraImpulseSource.GenerateImpulse();

			foreach (ParticleSystem p in secondaryParticles)
			{
				var mainModule = p.main;
				mainModule.startColor = c;
				p.Play();
			}
		}


		private void Speed(float x)
		{
			currentSpeed = x;
		}

		private void ChromaticAmount(float x)
		{
			chromaticAberration.intensity.value = x;
		}
	}
}