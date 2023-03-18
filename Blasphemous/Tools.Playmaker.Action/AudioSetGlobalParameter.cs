using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Set a global audio parameter.")]
public class AudioSetGlobalParameter : FsmStateAction
{
	public FsmString parameterName;

	public FsmFloat value;

	public override void Reset()
	{
		if (parameterName == null)
		{
			parameterName = new FsmString();
			parameterName.UseVariable = false;
			parameterName.Value = "DANGER";
		}
		if (value == null)
		{
			value = new FsmFloat(1f);
		}
	}

	public override void OnEnter()
	{
		string key = ((parameterName == null) ? string.Empty : parameterName.Value);
		float num = ((value == null) ? 0f : value.Value);
		Core.Audio.Ambient.SetSceneParam(key, num);
		Finish();
	}
}
