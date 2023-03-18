using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

[DefaultExecutionOrder(1)]
public class GlobalEmitter : AudioTool
{
	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[Range(0f, 10f)]
	private float startFadeTime;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[Range(0f, 10f)]
	private float startDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private AudioParam[] parameters = new AudioParam[0];

	protected override void BaseAwake()
	{
		base.BaseAwake();
		base.IsEmitter = true;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	protected override void BaseDestroy()
	{
		base.BaseDestroy();
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		if (base.gameObject.activeInHierarchy)
		{
		}
	}
}
