using System.Collections;
using UMA;
using UMA.CharacterSystem;
using UMA.PoseTools;
using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class UmaUepDriver : MonoBehaviour
	{
		private DynamicCharacterAvatar dynamicCharacterAvatar;
		public UMAExpressionPlayer expPlayer;
		public UmaUepProxy uepProxy;
		public PreviewModel previewModel = PreviewModel.Male;
		public enum PreviewModel
		{
			Male,
			Female
		}

		public bool useHead = true;
		public bool useEyes = true;
		public bool isDynamic = true;

		private bool umaIsReady = false;
		private float[] amounts;
		private IEnumerator coroWaitForDCA;

		void Start()
		{
			uepProxy = GetComponent<UmaUepProxy>();

			if (coroWaitForDCA != null)
				StopCoroutine(coroWaitForDCA);

			if (isDynamic)
			{
				coroWaitForDCA = WaitForUmaDCA();
				StartCoroutine(coroWaitForDCA);
			}
		}

		/// <summary>
		/// Call when the UMAExpressionPlayer (UEP) is completely configured. This will link
		/// up the UEP to this driver, initialize the driver, Eyes, release the driver to
		/// function.
		/// </summary>
		public void ManualStart(UMAExpressionPlayer uep)
		{
			expPlayer = uep;
			InitVars();
		}

//		/// <summary>
//		/// Update UepProxy with external values.
//		/// </summary>
//		/// <exception cref="NotImplementedException"></exception>
//		private void Update()
//		{
//			if (!umaIsReady)
//				return;
//
//			float[] amounts = expPlayer.Values;
//			for (int i = 0; i < amounts.Length; i++)
//			{
//				uepProxy.LooseSetPose(i, expPlayer.Values[i]);
//			}
//		}

		private IEnumerator WaitForUmaDCA()
		{
			while (dynamicCharacterAvatar == null)
			{
				yield return null;

				dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
			}

			dynamicCharacterAvatar.CharacterCreated.AddListener(new UnityEngine.Events.UnityAction<UMAData>(CharacterCreated));
		}

		/// <summary>
		/// Override UMAExpressionPlayer with UepProxy values.
		/// </summary>
		private void LateUpdate()
		{
			if (!umaIsReady && Application.isPlaying)
				return;

			UpdateExpressionPlayer();
		}

		public void UpdateExpressionPlayer()
		{
			for (int i = 0; i < uepProxy.Poses.Length; i++)
			{
				if (uepProxy.Poses[i].isDirty) // only write modified values.
				{
					amounts[i] = uepProxy.Poses[i].amount;
					uepProxy.ClearDirty(i); // UMA updated, clear the dirty flag.
				}
				else
					amounts[i] = expPlayer.Values[i];
			}

			expPlayer.Values = amounts;
		}

		public void CharacterCreated(UMAData umaData)
		{
			if (umaData.gameObject != gameObject)
				return; // character created is not this character.

			dynamicCharacterAvatar.animationController = umaData.animationController;
			if (dynamicCharacterAvatar.animationController)
			{
				var expSet = umaData.umaRecipe.raceData.expressionSet;
				if (!SetExpressionPlayer(umaData.gameObject))
					return;
				expPlayer.expressionSet = expSet;
				expPlayer.umaData = umaData;
				expPlayer.Initialize();
			}

			InitVars();
		}

		private bool SetExpressionPlayer(GameObject avatarGameObject)
		{
			if (!expPlayer)
				expPlayer = GetComponent<UMAExpressionPlayer>();
			if (!expPlayer)
				expPlayer = avatarGameObject.AddComponent<UMAExpressionPlayer>();
			if (!expPlayer)
			{
				Debug.LogError("UmaUepDriver: UMAExpressionPlayer NOT found -- and expression player is required for this OneClick solution.");

				return false;
			}

			return true;
		}

		public void InitVars()
		{
			amounts = new float[expPlayer.Values.Length];
			umaIsReady = true;

			// if using Eyes module for head and/or eyes, we will call their setup here (after the character
			// is created).
			if (useHead)
				OneClickUmaDcsEyes.ConfigureHead(gameObject);
			if (useEyes)
				OneClickUmaDcsEyes.ConfigureEyes(gameObject);

			// must be called once, only if either of the two above have been called
			if (useHead || useEyes)
				OneClickUmaDcsEyes.Initialize(gameObject);
		}
	}
}