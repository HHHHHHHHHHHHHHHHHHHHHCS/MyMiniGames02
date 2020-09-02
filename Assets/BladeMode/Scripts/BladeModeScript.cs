using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

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

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cutPlane.gameObject.SetActive(false);

		anim = GetComponent<Animator>();
		movement = GetComponent<MovementInput>();
		composers = new CinemachineComposer[3];
		for (int i = 0; i < 3; i++)
		{
			composers[i] = TPCamera.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
		}

		normalOffset = composers[0].m_TrackedObjectOffset;

		particles = cutPlane.GetComponentsInChildren<ParticleSystem>();

		cameraTs = Camera.main.transform;
		secondReference = cameraTs.transform.Find("SecondReference").transform;
	}

	private void Update()
	{
		anim.SetFloat("x", Mathf.Clamp(secondReference.localPosition.x + 0.3f, -1, 1));
		anim.SetFloat("y", Mathf.Clamp(secondReference.localPosition.y + 0.18f, -1, 1));
		
		if (Input.GetMouseButtonDown(1))
		{
			//Zoom(true);
		}

		if (Input.GetMouseButtonUp(1))
		{
			//Zoom(false);
		}

		if (bladeMode)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation,cameraTs.transform.rotation,.2f);
			//RotatePlane();

			if (Input.GetMouseButtonDown(0))
			{
				// cutPlane.GetChild(0).DOComplete();
				// cutPlane.GetChild(0).DOLocalMoveX(cutPlane.GetChild(0).localPosition.x * -1, .05f).SetEase(Ease.OutExpo);
				// ShakeCamera();
				// Slice();
			}
		}

		// Debug();
	}
}