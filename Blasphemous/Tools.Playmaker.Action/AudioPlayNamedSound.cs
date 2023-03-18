using System;
using FMOD.Studio;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Plays an name sound.")]
public class AudioPlayNamedSound : FsmStateAction
{
	public FsmString soundName;

	public FsmString eventName;

	private EventInstance audioEvent;

	public override void Reset()
	{
		if (soundName == null)
		{
			soundName = new FsmString(string.Empty);
		}
		if (eventName == null)
		{
			eventName = new FsmString(string.Empty);
		}
	}

	public override void OnEnter()
	{
		GameModeManager gameModeManager = Core.GameModeManager;
		gameModeManager.OnEnterMenuMode = (Core.SimpleEvent)Delegate.Combine(gameModeManager.OnEnterMenuMode, new Core.SimpleEvent(StopSound));
		PlaySound();
		Finish();
	}

	public void PlaySound()
	{
		string keyName = ((soundName == null) ? string.Empty : soundName.Value);
		string text = ((eventName == null) ? string.Empty : eventName.Value);
		Core.Audio.PlayNamedSound(text, keyName);
	}

	public void StopSound()
	{
		string keyName = ((soundName == null) ? string.Empty : soundName.Value);
		Core.Audio.StopNamedSound(keyName);
	}
}
