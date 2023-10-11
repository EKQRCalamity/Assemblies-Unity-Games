using UnityEngine;

public class ExitGameHook : MonoBehaviour
{
	public bool showConfirmationPopup = true;

	public bool parentToSceneSpaceUI = true;

	[SerializeField]
	[HideInInspectorIf("_hideUIParentOverride", false)]
	protected Transform _uiParentOverride;

	public Transform uiParent
	{
		get
		{
			if (!parentToSceneSpaceUI)
			{
				if (!_uiParentOverride)
				{
					return base.transform;
				}
				return _uiParentOverride;
			}
			return CameraManager.Instance.screenSpaceUICamera.GetComponentInChildren<Canvas>().transform;
		}
	}

	public void ExitGame()
	{
		if (showConfirmationPopup)
		{
			GameUtil.ExitApplicationPopup(null, uiParent, "Exit To Desktop", "Exit To Desktop");
		}
		else
		{
			GameUtil.ExitApplication();
		}
	}
}
