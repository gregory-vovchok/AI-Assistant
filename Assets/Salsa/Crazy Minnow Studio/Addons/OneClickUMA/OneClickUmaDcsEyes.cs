using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickUmaDcsEyes : MonoBehaviour
	{
		public static void Setup(GameObject umaGO)
		{
			Eyes eyes = umaGO.GetComponent<Eyes>();
			if (eyes == null)
			{
				eyes = umaGO.AddComponent<Eyes>();
			}
			else
			{
				DestroyImmediate(eyes);
				eyes = umaGO.AddComponent<Eyes>();
			}
			QueueProcessor qp = umaGO.GetComponent<QueueProcessor>();
			if (qp == null) qp = umaGO.AddComponent<QueueProcessor>();

			// System Properties
			eyes.characterRoot = umaGO.transform;
			eyes.queueProcessor = qp;

			// Eyelids - UMA
			eyes.BuildEyelidTemplate(Eyes.EyelidTemplates.UMA);
			eyes.SetEyelidShapeSelection(Eyes.EyelidSelection.Upper);
			float blinkAmount = -1;
			UmaUepProxy proxy = umaGO.GetComponent<UmaUepProxy>();
			if (proxy)
			{
				// Left eye
				eyes.eyelids[0].expData.controllerVars[0].umaUepProxy = proxy;
				eyes.eyelids[0].expData.controllerVars[0].blendIndex = proxy.GetPoseIndex("leftEyeOpen_Close");
				eyes.eyelids[0].expData.controllerVars[0].uepAmount = blinkAmount;
				// Right eye
				eyes.eyelids[1].expData.controllerVars[0].umaUepProxy = proxy;
				eyes.eyelids[1].expData.controllerVars[0].blendIndex = proxy.GetPoseIndex("rightEyeOpen_Close");
				eyes.eyelids[1].expData.controllerVars[0].uepAmount = blinkAmount;
			}
		}

		public static void ConfigureHead(GameObject umaGO)
		{
			string head = "^head$";
			Eyes eyes = umaGO.GetComponent<Eyes>();
			if (eyes)
			{
				eyes.BuildHeadTemplate(Eyes.HeadTemplates.Bone_Rotation_XY);
				eyes.heads[0].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, head);
				eyes.headTargetOffset.y = 0.1f;
				eyes.FixAllTransformAxes(ref eyes.heads, false);
				eyes.FixAllTransformAxes(ref eyes.heads, true);
				eyes.UpdateRuntimeExpressionControllers(ref eyes.heads);
			}
		}

		public static void ConfigureEyes(GameObject umaGO)
		{
			string eyeL = "^lefteye$";
			string eyeR = "^righteye$";

			Eyes eyes = umaGO.GetComponent<Eyes>();
			if (eyes)
			{
				eyes.BuildEyeTemplate(Eyes.EyeTemplates.Bone_Rotation);
				eyes.eyes[0].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, eyeL);
				eyes.eyes[1].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, eyeR);
				eyes.FixAllTransformAxes(ref eyes.eyes, false);
				eyes.FixAllTransformAxes(ref eyes.eyes, true);
				if (eyes.eyelidTemplate == Eyes.EyelidTemplates.UMA && eyes.eyelids.Count > 1)
				{
					eyes.eyelids[0].referenceIdx = 0;
					eyes.eyelids[1].referenceIdx = 1;
				}
				eyes.UpdateRuntimeExpressionControllers(ref eyes.eyes);
			}
		}

		public static void Initialize(GameObject umaGO)
		{
			Eyes eyes = umaGO.GetComponent<Eyes>();
			if (eyes) eyes.Initialize();
		}
	}
}