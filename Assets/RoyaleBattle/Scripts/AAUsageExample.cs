using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoyaleBattle
{
	public class AAUsageExample : MonoBehaviour
	{
		public AssetReferenceGameObject refObject;
		public AssetReference scene;

		private void Start()
		{
			refObject.InstantiateAsync(Vector3.zero, Quaternion.identity, null).Completed += OnAssetInstantiated;
		}

		private void OnAssetInstantiated(AsyncOperationHandle<GameObject> asyncOp)
		{
			Debug.Log(asyncOp.Result.name + " loaded.");
		}
	}
}