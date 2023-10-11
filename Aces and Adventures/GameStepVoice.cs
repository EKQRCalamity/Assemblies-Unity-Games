using System.Collections;
using UnityEngine;

public class GameStepVoice : GameStep
{
	public SoundPack soundPack;

	public float maxWaitTime;

	public GameStepVoice(SoundPack soundPack, float maxWaitTime = float.MaxValue)
	{
		this.soundPack = soundPack;
		this.maxWaitTime = maxWaitTime;
	}

	public override void Start()
	{
		if (!soundPack)
		{
			Cancel();
		}
	}

	protected override IEnumerator Update()
	{
		VoiceSource activeSource = VoiceManager.Instance.Play(base.state.player.view.transform, soundPack, interrupt: true);
		while (!activeSource.finished && (maxWaitTime -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
	}
}
