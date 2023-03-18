using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

[DefaultExecutionOrder(3)]
public class AmbientMusic : AudioTool
{
	private const float DOOR_TRANSITION_TIME = 1.5f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	public bool SceneWithAmbientMusc;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	protected float StartTime = 10f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	protected float EndTime = 300f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private AudioParamInitialized[] parameters = new AudioParamInitialized[0];

	private static DOTween currentTimeTransition;

	protected string CurrentAudioZone { get; set; }

	protected bool RunningCounter { get; private set; }

	protected override void BaseAwake()
	{
		base.BaseAwake();
		base.IsEmitter = false;
		RunningCounter = false;
	}

	protected override void BaseDestroy()
	{
		base.BaseDestroy();
	}
}
