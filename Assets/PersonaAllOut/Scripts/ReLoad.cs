using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PersonaAllOut
{
	public class ReLoad : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
			}
		}
	}
}