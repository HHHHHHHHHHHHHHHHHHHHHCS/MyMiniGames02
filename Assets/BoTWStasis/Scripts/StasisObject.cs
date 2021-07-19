using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace BoTWStasis.Scripts
{
	public class StasisObject : MonoBehaviour
	{
		private static readonly int StasisAmount_ID = Shader.PropertyToID("_StasisAmount");
		private static readonly int NoiseAmount_ID = Shader.PropertyToID("_NoiseAmount");
		private static readonly int EmissionColor_ID = Shader.PropertyToID("_EmissionColor");
		private static readonly int Color_ID = Shader.PropertyToID("_Color");

		private Rigidbody rb;
		private TrailRenderer trail;

		public bool activated;
		public Color particleColor;

		private float forceLimit = 100;
		public float accumulatedForce;
		public Vector3 direction;
		public Vector3 hitPoint;

		private Color normalColor;
		private Color finalColor;

		private Transform arrow;
		private Renderer renderer;

		[Header("Particles")] public Transform startparticleGroup;
		public Transform endParticleGroup;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			trail = GetComponent<TrailRenderer>();
			normalColor = StasisCharacter.instance.normalColor;
			finalColor = StasisCharacter.instance.finalColor;
			particleColor = normalColor;
			renderer = GetComponent<Renderer>();
		}

		public void SetEmissionColor(Color highlightedColor)
		{
			renderer.material.SetColor(EmissionColor_ID, highlightedColor);
		}

		public void SetStasis(bool state)
		{
			activated = state;
			rb.isKinematic = state;
			float noise = state ? 1 : 0;

			var mat = renderer.material;
			mat.SetFloat(StasisAmount_ID, 0.2f);
			mat.SetFloat(NoiseAmount_ID, noise);

			if (state)
			{
				mat.SetColor(EmissionColor_ID, normalColor);
				StartCoroutine(StasisWait());

				startparticleGroup.LookAt(StasisCharacter.instance.transform);
				var particles = startparticleGroup.GetComponentsInChildren<ParticleSystem>();
				foreach (var particle in particles)
				{
					particle.Play();
				}
			}
			else
			{
				StopAllCoroutines();
				DOTween.KillAll();
				transform.GetChild(0).gameObject.SetActive(false);
				renderer.material.SetFloat(StasisAmount_ID, 0);
				Destroy(arrow.gameObject);

				var particles = endParticleGroup.GetComponentsInChildren<ParticleSystem>();
				foreach (var particle in particles)
				{
					var pMain = particle.main;
					pMain.startColor = particleColor;
					particle.Play();
				}

				if (accumulatedForce <= 0)
				{
					return;
				}

				direction = transform.position - hitPoint;
				rb.AddForceAtPosition(direction * accumulatedForce, hitPoint, ForceMode.Impulse);
				accumulatedForce = 0;

				trail.startColor = particleColor;
				trail.endColor = particleColor;
				trail.emitting = true;
				trail.DOTime(0, 5).OnComplete(() => trail.emitting = false);
			}
		}

		public void AccumulateForce(float amount, Vector3 point)
		{
			if (!activated)
			{
				return;
			}

			if (arrow == null)
			{
				arrow = Instantiate(StasisCharacter.instance.arrow, transform).transform;
			}

			float scale = Mathf.Min(arrow.localScale.z + 0.3f, 1.8f);
			arrow.DOScaleZ(scale, 0.15f).SetEase(Ease.OutBack);

			accumulatedForce = Mathf.Min(accumulatedForce += amount, forceLimit);
			hitPoint = point;

			direction = transform.position - hitPoint;
			arrow.rotation = Quaternion.LookRotation(direction);

			Color c = Color.Lerp(normalColor, finalColor, accumulatedForce / 50.0f);
			arrow.GetComponentInChildren<Renderer>().material.SetColor(Color_ID, c);
			renderer.material.SetColor(EmissionColor_ID, c);
			particleColor = c;
		}

		private IEnumerator StasisWait()
		{
			for (int i = 0; i < 20; i++)
			{
				float wait;

				if (i > 12)
				{
					wait = 0.25f;
				}
				else if (i > 4)
				{
					wait = 0.5f;
				}
				else
				{
					wait = 1;
				}

				yield return new WaitForSeconds(wait);
				Sequence s = DOTween.Sequence();
				s.Append(renderer.material.DOFloat(0.5f, StasisAmount_ID, 0.05f));
				s.AppendInterval(0.1f);
				s.Append(renderer.material.DOFloat(0.2f, StasisAmount_ID, 0.05f));
			}

			SetStasis(false);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position - direction);
		}
	}
}