using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

[DefaultExecutionOrder(1)]
public class SceneAudio : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Global", true, false, 0)]
	private AudioParam[] globalParameters = new AudioParam[0];

	[SerializeField]
	[BoxGroup("Global", true, false, 0)]
	[EventRef]
	private string trackIdentifier;

	[SerializeField]
	[BoxGroup("Global", true, false, 0)]
	[EventRef]
	private string idReverb;

	[SerializeField]
	[BoxGroup("Ambient", true, false, 0)]
	private AudioParamInitialized[] ambientParameters = new AudioParamInitialized[0];

	[SerializeField]
	[BoxGroup("Ambient", true, false, 0)]
	protected float StartTime = 10f;

	[SerializeField]
	[BoxGroup("Ambient", true, false, 0)]
	protected float EndTime = 300f;

	private void Awake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Core.Audio.Ambient.SetSceneParams(trackIdentifier, idReverb, globalParameters, newLevel.LevelName);
			Core.Audio.Ambient.SetAmbientParams(ambientParameters, StartTime, EndTime);
		}
	}

	public void RestartSceneAudio()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Core.Audio.Ambient.StopCurrent();
			Core.Audio.Ambient.SetSceneParams(trackIdentifier, idReverb, globalParameters, Core.LevelManager.currentLevel.LevelName);
			Core.Audio.Ambient.SetAmbientParams(ambientParameters, StartTime, EndTime);
		}
	}
}
