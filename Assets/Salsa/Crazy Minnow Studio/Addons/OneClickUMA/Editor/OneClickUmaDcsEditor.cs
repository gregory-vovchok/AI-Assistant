using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	/// <summary>
	/// RELEASE NOTES:
	/// 	2.1.0 (2019-08-25):
	/// 		+ Experimental support for non-dynamic UMA avatars (assumes a
	/// 			correctly configured UMAExpressionPlayer).
	/// 	2.0.0 (2019-07-20):
	/// 		- confirmed operation with Base 2.1.5
	///			+ Initial release.
	/// ==========================================================================
	/// PURPOSE: This script provides simple, simulated lip-sync input to the
	///		Salsa component from text/string values. For the latest information
	///		visit crazyminnowstudio.com.
	/// ==========================================================================
	/// DISCLAIMER: While every attempt has been made to ensure the safe content
	///		and operation of these files, they are provided as-is, without
	///		warranty or guarantee of any kind. By downloading and using these
	///		files you are accepting any and all risks associated and release
	///		Crazy Minnow Studio, LLC of any and all liability.
	/// ==========================================================================
	/// </summary>
	public class OneClickUmaDcsEditor : MonoBehaviour
	{
		[MenuItem("GameObject/Crazy Minnow Studio/SALSA LipSync/One-Clicks/UMA DCS")]
		public static void UmaDcsSetup()
		{
			GameObject go = Selection.activeGameObject;

#if UNITY_2018_3_OR_NEWER
				if (PrefabUtility.IsPartOfAnyPrefab(go))
				{
					if (EditorUtility.DisplayDialog(
													OneClickBase.PREFAB_ALERT_TITLE,
													OneClickBase.PREFAB_ALERT_MSG,
													"YES", "NO"))
					{
						PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
						ApplyOneClick(go);
					}
				}
				else
				{
					ApplyOneClick(go);
				}
#else
			ApplyOneClick(go);
#endif
		}

		private static void ApplyOneClick(GameObject go)
		{
			var uepDriver = go.GetComponent<UmaUepDriver>();
			if (uepDriver == null)
				go.AddComponent<UmaUepDriver>();

			var uepProxy = go.GetComponent<UmaUepProxy>();
			if (uepProxy == null)
				go.AddComponent<UmaUepProxy>();

			OneClickUmaDcs.Setup(go, AssetDatabase.LoadAssetAtPath<AudioClip>(OneClickBase.RESOURCE_CLIP));
			OneClickUmaDcsEyes.Setup(Selection.activeGameObject);
		}
	}
}