using UMA;
using UMA.PoseTools;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	[ExecuteInEditMode]
	public class UmaUepDriverEditorPreview : MonoBehaviour
	{
		public UMAExpressionPlayer expPlayer;
		public UMAExpressionSet expSet;
		public Transform skeletonRoot;
		public Transform twirler;
		[HideInInspector] public UMASkeleton skeleton;
		private UmaUepDriver uepDriver;

		public void Start()
		{
			uepDriver = transform.parent.GetComponent<UmaUepDriver>();
			uepDriver.uepProxy = transform.parent.GetComponent<UmaUepProxy>();
			uepDriver.expPlayer = expPlayer;
			uepDriver.InitVars();
			uepDriver.expPlayer.expressionSet = expSet;
			skeleton = new UMASkeleton(skeletonRoot);
		}
		void OnRenderObject()
		{
			if (expSet == null) return;
			if (skeleton == null) return;

			expSet.RestoreBones(skeleton);
		}

		void Update()
		{
//			if (!uepDriver || !uepDriver.uepProxy || !uepDriver.expPlayer) return;

			uepDriver.UpdateExpressionPlayer();
			UpdatePreview();
		}

		void UpdatePreview()
		{
			if (expSet == null) return;
			if (skeletonRoot == null) return;

			expSet.RestoreBones(skeleton);

			float[] values = uepDriver.expPlayer.Values;

			for (int i = 0; i < values.Length; i++)
			{
				float weight = values[i];

				UMABonePose pose = null;
				if (weight > 0)
				{
					pose = expSet.posePairs[i].primary;
				}
				else
				{
					weight = -weight;
					pose = expSet.posePairs[i].inverse;
				}

				if (pose == null) continue;

				pose.ApplyPose(skeleton, weight);
			}
		}
	}
}