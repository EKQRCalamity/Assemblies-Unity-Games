using Framework.Managers;
using Gameplay.UI;
using Tools.Level.Interactables;
using UnityEngine;

public class FinalWaypoint : MonoBehaviour
{
	public string impactAudio;

	public float impactTiming = 0.5f;

	public float sceneChangeTiming = 1f;

	public float audioTiming = 0.35f;

	private void Start()
	{
		GetComponent<PrieDieu>().OnStartUsing += OnUseStart;
	}

	private void OnUseStart()
	{
		Core.Metrics.CustomEvent("GAME_COMPLETED", string.Empty, Time.time);
		Core.Audio.PlaySfxOnCatalog(impactAudio, audioTiming);
		Invoke("Fade", impactTiming);
		Invoke("FinishPrototype", sceneChangeTiming);
	}

	private void Fade()
	{
		UIController.instance.fade.CrossFadeAlpha(1f, 0f, ignoreTimeScale: false);
	}

	private void FinishPrototype()
	{
		Core.Logic.LoadMenuScene();
	}
}
