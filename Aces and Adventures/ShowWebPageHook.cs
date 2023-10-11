using UnityEngine;

public class ShowWebPageHook : MonoBehaviour
{
	public string url;

	public string friendlyURL;

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

	public void ShowWebPage()
	{
		Job.Process(UIUtil.ShowWebBrowser(url, uiParent, friendlyURL));
	}
}
