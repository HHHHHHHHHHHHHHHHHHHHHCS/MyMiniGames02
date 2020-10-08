using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PropertyCopyPaste.Editor
{
	public static class PropertyCopyPaste
	{
		[MenuItem("Tools/Property/Copy &#c")]
		public static void Copy()
		{
			var gos = Selection.gameObjects;
			if (gos != null && gos.Length >= 1 && gos[0] != null)
			{
				var ts = gos[0].transform;
				TransformToJson(ts);
			}
		}

		[MenuItem("Tools/Property/Paste &#v")]
		public static void Paste()
		{
			var gos = Selection.gameObjects;
			if (gos != null && gos.Length >= 1 && gos[0] != null)// && AssetDatabase.IsMainAsset(gos[0]))
			{
				JsonToTransform(gos[0].transform);
			}
		}

		private static void TransformToJson(Transform ts)
		{
			if (ts == null)
			{
				return;
			}

			StringBuilder sb = new StringBuilder();

			sb.Append(ts.name).Append(',');
			var pos = ts.position;
			sb.Append(pos.x).Append(',').Append(pos.y).Append(',').Append(pos.z).Append(',');
			var rot = ts.rotation.eulerAngles;
			sb.Append(rot.x).Append(',').Append(rot.y).Append(',').Append(rot.z).Append(',');
			var sca = ts.localScale;
			sb.Append(sca.x).Append(',').Append(sca.y).Append(',').Append(sca.z).Append(',');

			if (ts is RectTransform rt)
			{
				var size = rt.sizeDelta;
				sb.Append(size.x).Append(',').Append(size.y).Append(',');
				var ancMin = rt.anchorMin;
				sb.Append(ancMin.x).Append(',').Append(ancMin.y).Append(',');
				var ancMax = rt.anchorMax;
				sb.Append(ancMax.x).Append(',').Append(ancMax.y).Append(',');
				var pivot = rt.pivot;
				sb.Append(pivot.x).Append(',').Append(pivot.y).Append(',');
			}

			GUIUtility.systemCopyBuffer = sb.ToString();
		}

		private static void JsonToTransform(Transform ts)
		{
			if (ts == null)
			{
				return;
			}

			string data = GUIUtility.systemCopyBuffer;

			if (data?.Length == 0)
			{
				return;
			}

			var datas = data.Split(',');

			if (datas.Length < 11)
			{
				return;
			}

			try
			{
				Undo.RecordObject(ts,"Paste");
				ts.name = datas[0];

				//先设置锚点确保位置正确
				if (datas.Length > 11 && ts is RectTransform rt)
				{
					rt.sizeDelta = new Vector2(float.Parse(datas[10]), float.Parse(datas[11]));
					rt.anchorMin = new Vector2(float.Parse(datas[12]), float.Parse(datas[13]));
					rt.anchorMax = new Vector2(float.Parse(datas[14]), float.Parse(datas[15]));
					rt.pivot = new Vector2(float.Parse(datas[16]), float.Parse(datas[17]));
				}
				
				ts.position = new Vector3(float.Parse(datas[1]), float.Parse(datas[2]), float.Parse(datas[3]));
				ts.rotation = Quaternion.Euler(float.Parse(datas[4]), float.Parse(datas[5]), float.Parse(datas[6]));
				ts.localScale = new Vector3(float.Parse(datas[7]), float.Parse(datas[8]), float.Parse(datas[9]));
			}
			catch (Exception e)
			{
				Debug.LogError(e.Data);
				throw;
			}
		}
	}
}