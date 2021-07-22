using System;
using UnityEngine;

namespace CelesteMovement.Scripts
{
	public class RippleEffect : MonoBehaviour
	{
		public class Droplet
		{
			private Vector2 position;
			private float time;

			public bool IsEnd { get; private set; }

			public Droplet()
			{
				time = 1000;
				IsEnd = true;
			}

			public void Reset(Vector2 pos)
			{
				position = pos;
				time = 0;
				IsEnd = false;
			}

			public bool DoUpdate()
			{
				if (IsEnd)
				{
					return false;
				}

				time += Time.deltaTime * 2;
				if (time > 1000)
				{
					IsEnd = true;
				}

				return true;
			}

			public Vector4 MakeShaderParameter(float aspect)
			{
				return new Vector4(position.x * aspect, position.y, time, IsEnd ? 0 : 1);
			}
		}


		public AnimationCurve waveform = new AnimationCurve(
			new Keyframe(0.00f, 0.50f, 0, 0),
			new Keyframe(0.05f, 1.00f, 0, 0),
			new Keyframe(0.15f, 0.10f, 0, 0),
			new Keyframe(0.25f, 0.80f, 0, 0),
			new Keyframe(0.35f, 0.30f, 0, 0),
			new Keyframe(0.45f, 0.60f, 0, 0),
			new Keyframe(0.55f, 0.40f, 0, 0),
			new Keyframe(0.65f, 0.55f, 0, 0),
			new Keyframe(0.75f, 0.46f, 0, 0),
			new Keyframe(0.85f, 0.52f, 0, 0),
			new Keyframe(0.99f, 0.50f, 0, 0)
		);

		[Range(0.01f, 1.0f)] public float refractionStrength = 0.5f;

		public Color reflectionColor = Color.gray;

		[Range(0.01f, 1.0f)] public float reflectionStrength = 0.7f;

		[Range(1.0f, 5.0f)] public float waveSpeed = 1.25f;


		[SerializeField] private Shader shader;


		private Droplet[] droplets;
		private Texture2D gradTexture;
		private Material material;
		private int dropCount;

		private void Awake()
		{
			droplets = new Droplet[3];
			for (int i = 0; i < droplets.Length; i++)
			{
				droplets[i] = new Droplet();
			}


			gradTexture = new Texture2D(2048, 1, TextureFormat.R8, false)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Bilinear,
			};
			var cols = new Color[gradTexture.width];
			float step = 1.0f / gradTexture.width;
			float val = 0;
			for (var i = 0; i < gradTexture.width; i++)
			{
				var r = waveform.Evaluate(val);
				cols[i] = new Color(r, r, r, r);
				val += step;
			}

			gradTexture.SetPixels(cols);
			gradTexture.Apply();

			material = new Material(shader);
			material.hideFlags = HideFlags.DontSave;
			material.SetTexture("_GradTex", gradTexture);

			UpdateShaderParameters();
		}

		private void OnDestroy()
		{
			Destroy(material);
			Destroy(gradTexture);
		}

		private void Update()
		{
			bool needUpdate = false;

			foreach (var droplet in droplets)
			{
				needUpdate |= droplet.DoUpdate();
			}

			if (needUpdate)
			{
				UpdateShaderParameters();
			}
		}

		private void UpdateShaderParameters()
		{
			var aspect = Camera.main.aspect;

			material.SetVector("_Drop1", droplets[0].MakeShaderParameter(aspect));
			material.SetVector("_Drop2", droplets[1].MakeShaderParameter(aspect));
			material.SetVector("_Drop3", droplets[2].MakeShaderParameter(aspect));

			material.SetColor("_Reflection", reflectionColor);
			material.SetVector("_Params1", new Vector4(aspect, 1, 1 / waveSpeed, 0));
			material.SetVector("_Params2", new Vector4(1, 1 / aspect, refractionStrength, reflectionStrength));
		}
		
		public void Emit(Vector2 pos)
		{
			droplets[dropCount++ % droplets.Length].Reset(pos);
		}
	}
}