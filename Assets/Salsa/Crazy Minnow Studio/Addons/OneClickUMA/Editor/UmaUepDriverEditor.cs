using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	[CustomEditor(typeof(UmaUepDriver))]
	public class UmaUepDriverEditor : Editor
	{
		private UmaUepDriver uepDriver;
		private const string MALE_PREFAB = "Assets/Crazy Minnow Studio/Addons/OneClickUMA/OneClickUmaPreviewRigMale.prefab";
		private const string FEMALE_PREFAB = "Assets/Crazy Minnow Studio/Addons/OneClickUMA/OneClickUmaPreviewRigFemale.prefab";
		private GUIStyle stylewrap = new GUIStyle();

		private void OnEnable()
		{
			uepDriver = target as UmaUepDriver;
			uepDriver.uepProxy = uepDriver.GetComponent<UmaUepProxy>();
		}

		public override void OnInspectorGUI()
		{
			if (uepDriver.uepProxy.isPreviewing)
				EnablePreview();
			else
				DisablePreview();

			uepDriver.isDynamic = GUILayout.Toggle(uepDriver.isDynamic,
				new GUIContent("UMA Character is Dynamic",
					"Leave this enabled for dynamic UMA character avatars."));


			GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical(GUILayout.MaxHeight(5f));
						GUILayout.FlexibleSpace();
						GUILayout.Label("Options for Eyes Module:");

						GUILayout.BeginHorizontal();
							GUILayout.Space(15f);
							uepDriver.useHead = GUILayout.Toggle(uepDriver.useHead,
																 new GUIContent("Use Head",
																				"Enable to leverage OneClick setup for Head."));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
							GUILayout.Space(15f);
							uepDriver.useEyes = GUILayout.Toggle(uepDriver.useEyes,
																 new GUIContent("Use Eyes",
																				"Enable to leverage OneClick setup for eyes."));
						GUILayout.EndHorizontal();
						GUILayout.FlexibleSpace();
					GUILayout.EndVertical();


					InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Warning);
					GUILayout.BeginVertical(GUILayout.MaxHeight(50f));
						GUILayout.FlexibleSpace();
						stylewrap.wordWrap = true;
						stylewrap.fontStyle = FontStyle.Bold;
						stylewrap.alignment = TextAnchor.MiddleCenter;
						GUILayout.BeginHorizontal(EditorStyles.helpBox);
						GUILayout.Label("Leave this component open to enable preview mode for UMA.", stylewrap);
						GUILayout.EndHorizontal();
						GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					InspectorCommon.DrawResetBg();
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent("Using " + (uepDriver.previewModel == UmaUepDriver.PreviewModel.Male ? "Male" : "Female") + " Preview Model", "Click to switch models."), GUILayout.ExpandWidth(false)))
			{
				if (uepDriver.previewModel == UmaUepDriver.PreviewModel.Male)
					uepDriver.previewModel = UmaUepDriver.PreviewModel.Female;
				else
					uepDriver.previewModel = UmaUepDriver.PreviewModel.Male;

				if (uepDriver.uepProxy.isPreviewing)
				{
					// reset preview by removing old preview prefab and enabling new preview prefab
					DisablePreview();
					EnablePreview();
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void DisablePreview()
		{
			var prefabComponent = uepDriver.GetComponentInChildren<UmaUepDriverEditorPreview>();
			if (prefabComponent != null)
				DestroyImmediate(prefabComponent.gameObject);
		}

		private void EnablePreview()
		{
			if (uepDriver.GetComponentInChildren<UmaUepDriverEditorPreview>() == null)
			{
				var previewer =
					(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase
								 				.LoadAssetAtPath<Object>(GetPreviewModelPrefab()));
				previewer.transform.SetParent(uepDriver.transform);
				previewer.transform.localPosition = Vector3.zero;
				var rot = previewer.transform.eulerAngles;
				previewer.transform.eulerAngles = new Vector3(rot.x, 0f, rot.z);
				previewer.transform.localScale = Vector3.one;

			}
		}

		private string GetPreviewModelPrefab()
		{
			switch (uepDriver.previewModel)
			{
				case UmaUepDriver.PreviewModel.Female:
					return FEMALE_PREFAB;
				case UmaUepDriver.PreviewModel.Male:
					return MALE_PREFAB;
				default:
					return MALE_PREFAB;
			}
		}
	}
}