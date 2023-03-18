using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Sets the camera in free mode and sets a position.")]
public class CameraModeFree : FsmStateAction
{
	public FsmGameObject Target;

	public override void Reset()
	{
		if (Target == null)
		{
			Target = new FsmGameObject();
			Target.Value = null;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Vector3 freeCameraPosition = ((Target == null || !(Target.Value != null)) ? Vector3.zero : Target.Value.transform.position);
		Core.Cinematics.SetFreeCamera(freeCamera: true);
		Core.Cinematics.SetFreeCameraPosition(freeCameraPosition);
		Finish();
	}
}
