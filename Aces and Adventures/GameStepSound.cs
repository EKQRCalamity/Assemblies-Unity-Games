using System.Collections;
using UnityEngine;

public class GameStepSound : GameStep
{
	public AudioRef audio;

	public float maxWaitTime;

	public GameStepSound(AudioRef audio, float maxWaitTime = 0f)
	{
		this.audio = audio;
		this.maxWaitTime = maxWaitTime;
	}

	public override void Start()
	{
		if (!audio)
		{
			Cancel();
		}
	}

	protected override IEnumerator Update()
	{
		VoiceSource activeSource = VoiceManager.Instance.Play(new SoundPack.SoundData().SetAudioRef(audio), interrupt: false, 0f, isGlobal: false);
		while (!activeSource.finished && (maxWaitTime -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
	}
}
