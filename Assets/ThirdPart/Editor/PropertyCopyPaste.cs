using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThirdPart.Editor
{
	public static class PropertyCopyPaste
	{
		[MenuItem("Tools/Property/Copy &#c")]
		public static void Copy()
		{
			var gos = Selection.gameObjects;
			if (gos != null && gos.Length >= 1)
			{
				var ts = gos[0].transform;
				var ss = JsonUtility.ToJson(ts);
				Debug.Log(ss);
			}
		}

		[MenuItem("Tools/Property/Paste &#v")]
		public static void Paste()
		{
			var gos = Selection.gameObjects;
			if (gos != null && gos.Length >= 1)
			{
				if (GUIUtility.systemCopyBuffer.Length > 0)
				{
					var ts = gos[0].transform;
					EditorJsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, ts.transform);
				}
			}
		}

		private static void ToJson(Transform ts)
		{
			StringBuilder sb = new StringBuilder();
			if (ts is RectTransform)
			{
				//TODO:
			}
			else
			{
				
			}
		}
	}
}