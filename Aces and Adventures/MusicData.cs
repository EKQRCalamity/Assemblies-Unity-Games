using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class MusicData : IDataContent
{
	private const double MAX_TIME = 999.0;

	[ProtoMember(1)]
	[UIField(filter = AudioCategoryType.Music, validateOnChange = true)]
	private AudioRef _music;

	[ProtoMember(2)]
	[UIField(min = 0, max = 999.0, view = "UI/Input Field Standard", tooltip = "Defines moment when intro is considered finished and looping section of song begins.")]
	private double _introLoopBoundary;

	[ProtoMember(3)]
	[UIField(min = 0, max = 999.0, view = "UI/Input Field Standard", tooltip = "Defines when the looping section of song is considered finished and a new loop should begin.\n<i>If loop ends at end of clip, leave number very large.</i>")]
	[DefaultValue(999.0)]
	private double _loopBoundary = 999.0;

	public AudioRef music => _music;

	public double introLoopBoundary => _introLoopBoundary;

	public double loopBoundary => Math.Min(_loopBoundary, _music.audioClip.Length());

	[ProtoMember(15)]
	public string tags { get; set; }

	private bool _hideButtons => !_music;

	public string GetTitle()
	{
		return _music.GetFriendlyName();
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		if (!_music)
		{
			return "Please make sure to select a sound clip for music.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}

	public static implicit operator bool(MusicData musicData)
	{
		if (musicData != null)
		{
			return musicData._music;
		}
		return false;
	}

	[UIField]
	[UIHideIf("_hideButtons")]
	private void _PlayFromBeginning()
	{
		MusicManager.Instance.Play(_music.audioClip, _introLoopBoundary, loopBoundary);
	}

	[UIField]
	[UIHideIf("_hideButtons")]
	private void _PlayIntroLoopSection()
	{
		MusicManager instance = MusicManager.Instance;
		AudioClip audioClip = _music.audioClip;
		double num = _introLoopBoundary;
		double num2 = loopBoundary;
		float? fadeInTimeOverride = 2f;
		double? playFromOverride = _introLoopBoundary - 3.0;
		instance.Play(audioClip, num, num2, MusicPlayType.Play, 1f, fadeInTimeOverride, null, playFromOverride).Stop(6f);
	}

	[UIField]
	[UIHideIf("_hideButtons")]
	private void _PlayEndLoopSection()
	{
		MusicManager instance = MusicManager.Instance;
		AudioClip audioClip = _music.audioClip;
		double num = _introLoopBoundary;
		double num2 = loopBoundary;
		float? fadeInTimeOverride = 2f;
		double? playFromOverride = Math.Min(_music.audioClip.Length(), loopBoundary) - 3.0;
		instance.Play(audioClip, num, num2, MusicPlayType.Play, 1f, fadeInTimeOverride, null, playFromOverride).Stop(6f);
	}

	[UIField]
	[UIHideIf("_hideButtons")]
	private void _StopMusic()
	{
		MusicManager.Instance.Stop();
	}
}
