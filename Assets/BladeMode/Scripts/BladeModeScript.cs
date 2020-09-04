using Cinemachine;
using DG.Tweening;
using EzySlice;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BladeMode
{
	public class BladeModeScript : MonoBehaviour
	{
		public bool bladeMode;

		private Animator anim;
		private MovementInput movement;
		private Vector3 normalOffset;
		public Vector3 zoomOffset;
		private float normalFOV;
		public float zoomFOV = 15;

		public Transform cutPlane;

		public CinemachineFreeLook TPCamera;

		public Material crossMaterial;

		public LayerMask layerMask;

		private CinemachineComposer[] composers;
		private ParticleSystem[] particles;
		private Transform cameraTs;
		private Transform secondReference;
		private VolumeProfile profile;
		private Transform planeReference;

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			cutPlane.gameObject.SetActive(false);

			anim = GetComponent<Animator>();
			movement = GetComponent<MovementInput>();
			normalFOV = TPCamera.m_Lens.FieldOfView;
			composers = new CinemachineComposer[3];
			for (int i = 0; i < 3; i++)
			{
				composers[i] = TPCamera.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
			}

			normalOffset = composers[0].m_TrackedObjectOffset;

			particles = cutPlane.GetComponentsInChildren<ParticleSystem>();

			cameraTs = Camera.main.transform;
			secondReference = cameraTs.transform.Find("SecondReference").transform;

			profile = cameraTs.GetComponentInChildren<Volume>().profile;

			planeReference = cutPlane.GetChild(0);
		}

		private void Update()
		{
			anim.SetFloat("x", Mathf.Clamp(secondReference.localPosition.x + 0.3f, -1, 1));
			anim.SetFloat("y", Mathf.Clamp(secondReference.localPosition.y + 0.18f, -1, 1));

			if (Input.GetMouseButtonDown(1))
			{
				Zoom(true);
			}

			if (Input.GetMouseButtonUp(1))
			{
				Zoom(false);
			}

			if (bladeMode)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, cameraTs.transform.rotation, .2f);
				RotatePlane();

				if (Input.GetMouseButtonDown(0))
				{
					planeReference.DOComplete();
					planeReference.DOLocalMoveX(planeReference.localPosition.x * -1, .05f).SetEase(Ease.OutExpo);
					ShakeCamera();
					Slice();
				}
			}

			ReLod();
		}


		public void Zoom(bool state)
		{
			bladeMode = state;
			anim.SetBool("bladeMode", bladeMode);

			cutPlane.localEulerAngles = Vector3.zero;
			cutPlane.gameObject.SetActive(state);

			//剑道模式下   旋转用 WSAD 而不是鼠标
			string x = state ? "Horizontal" : "Mouse X";
			string y = state ? "Vertical" : "Mouse Y";
			TPCamera.m_XAxis.m_InputAxisName = x;
			TPCamera.m_YAxis.m_InputAxisName = y;

			float fov = state ? zoomFOV : normalFOV;
			Vector3 offset = state ? zoomOffset : normalOffset;
			float timeScale = state ? 0.2f : 1f;

			DOVirtual.Float(Time.timeScale, timeScale, .02f, SetTimeScale);
			DOVirtual.Float(TPCamera.m_Lens.FieldOfView, fov, .1f, SetFieldOfView);
			//如果是SetUpdate true  忽略unity 的timescale
			DOVirtual.Float(composers[0].m_TrackedObjectOffset.x, offset.x, .2f, SetCameraOffset).SetUpdate(true);

			movement.enabled = !state;

			if (!state)
			{
				//感觉意义不大
				transform.DORotate(new Vector3(0, transform.eulerAngles.y, 0), 0.2f);
			}

			//POST PROCESSING
			float vig = state ? .6f : 0;
			float chrom = state ? 1 : 0;
			float depth = state ? 4.8f : 8;
			float vig2 = state ? 0f : .6f;
			float chrom2 = state ? 0 : 1;
			float depth2 = state ? 8 : 4.8f;
			DOVirtual.Float(chrom2, chrom, .1f, SetChromatic);
			DOVirtual.Float(vig2, vig, .1f, SetVignette);
			DOVirtual.Float(depth2, depth, .1f, SetDepthOfField);
			if (profile.TryGet(typeof(ColorAdjustments), out ColorAdjustments ca))
			{
				ca.active = state;
			}
		}

		public void RotatePlane()
		{
			cutPlane.eulerAngles += new Vector3(0, 0, -Input.GetAxis("Mouse X") * 5f);
		}

		public void ShakeCamera()
		{
			TPCamera.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
			foreach (ParticleSystem p in particles)
			{
				p.Play();
			}
		}

		public void Slice()
		{
			Collider[] hits = Physics.OverlapBox(cutPlane.position, new Vector3(5, 0.1f, 5f), cutPlane.rotation, layerMask);

			if (hits.Length <= 0)
			{
				return;
			}

			for (int i = 0; i < hits.Length; i++)
			{
				SlicedHull hull = SliceObject(hits[i].gameObject, crossMaterial);
				if (hull != null)
				{
					GameObject bottom = hull.CreateLowerHull(hits[i].gameObject, crossMaterial);
					GameObject top = hull.CreateUpperHull(hits[i].gameObject, crossMaterial);
					AddHullComponents(bottom);
					AddHullComponents(top);
					Destroy(hits[i].gameObject);
				}
			}
		}

		public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
		{
			if (obj.GetComponent<MeshFilter>() == null)
			{
				return null;
			}

			return obj.Slice(cutPlane.position, cutPlane.up, crossSectionMaterial);
		}

		public void AddHullComponents(GameObject go)
		{
			//go.layer = LayerMask.NameToLayer("Cuttable");
			go.layer = (int) Mathf.Log(layerMask.value, 2f);
			Rigidbody rb = go.AddComponent<Rigidbody>();
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			MeshCollider collider = go.AddComponent<MeshCollider>();
			//可能会大于256 警告
			collider.convex = true;

			rb.AddExplosionForce(100, go.transform.position, 20);
		}


		private void ReLod()
		{
			if (Input.GetKeyDown(KeyCode.G))
			{
				UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager
					.GetActiveScene().name);
			}
		}

		private void SetTimeScale(float time)
		{
			Time.timeScale = time;
		}

		private void SetFieldOfView(float fov)
		{
			TPCamera.m_Lens.FieldOfView = fov;
		}

		private void SetCameraOffset(float x)
		{
			foreach (var c in composers)
			{
				c.m_TrackedObjectOffset.Set(x, c.m_TrackedObjectOffset.y, c.m_TrackedObjectOffset.z);
			}
		}

		void SetChromatic(float x)
		{
			if (profile.TryGet(typeof(ChromaticAberration), out ChromaticAberration ca))
			{
				Debug.Log(x);
				ca.intensity.value = x;
			}
		}

		void SetVignette(float x)
		{
			if (profile.TryGet(typeof(Vignette), out Vignette vignette))
			{
				vignette.intensity.value = x;
			}
		}

		void SetDepthOfField(float x)
		{
			if (profile.TryGet(typeof(DepthOfField), out DepthOfField dof))
			{
				dof.aperture.value = x;
			}
		}
	}
}