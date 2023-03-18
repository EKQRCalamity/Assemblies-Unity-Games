using Framework.FrameworkCore;
using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[HutongGames.PlayMaker.Tooltip("Changes the current main camera.")]
public class ChangeCamera : FsmStateAction
{
	public enum ChangeCameraOptions
	{
		SET_CAMERA,
		RETURN_TO_NORMAL
	}

	public ChangeCameraOptions action;

	public FsmGameObject camera;

	public override void OnEnter()
	{
		if (action == ChangeCameraOptions.SET_CAMERA)
		{
			SetCamera();
		}
		else if (action == ChangeCameraOptions.RETURN_TO_NORMAL)
		{
			ReturnToNormal();
		}
	}

	private void SetCamera()
	{
		if (!Core.ready)
		{
			Framework.FrameworkCore.Log.Error("Playmaker", "Framework not initialized yet.");
			Finish();
			return;
		}
		if (camera.Value == null)
		{
			Framework.FrameworkCore.Log.Error("Playmaker", "The selected camera is null.");
			Finish();
			return;
		}
		Camera component = camera.Value.GetComponent<Camera>();
		if (component == null)
		{
			Framework.FrameworkCore.Log.Error("Playmaker", "The selected object has no camera component.");
			Finish();
		}
		else
		{
			Finish();
		}
	}

	private void ReturnToNormal()
	{
	}
}
