using FMOD.Studio;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Stops an name sound.")]
public class AudioStopNamedSound : FsmStateAction
{
	public FsmString soundName;

	public FMOD.Studio.STOP_MODE stopMode;

	public override void Reset()
	{
		if (soundName == null)
		{
			soundName = new FsmString(string.Empty);
		}
	}

	public override void OnEnter()
	{
		string keyName = ((soundName == null) ? string.Empty : soundName.Value);
		Core.Audio.StopNamedSound(keyName, stopMode);
		Finish();
	}
}
