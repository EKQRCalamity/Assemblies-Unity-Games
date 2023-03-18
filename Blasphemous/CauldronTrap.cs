using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

public class CauldronTrap : MonoBehaviour, IActionable
{
	[FoldoutGroup("References", 0)]
	public Animator trapAnimator;

	[FoldoutGroup("References", 0)]
	public Animator beamAnimator;

	[FoldoutGroup("References", 0)]
	public TileableBeamLauncher beam;

	[FoldoutGroup("References", 0)]
	public Transform origin;

	[FoldoutGroup("Design settings", 0)]
	public float fallDelay = 0.3f;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string FallAudio;

	private EventInstance _fallAudioEvent;

	public float timeActive = 5f;

	public float maxSeconds = 1f;

	public bool TrapActivated { get; set; }

	public bool Locked { get; set; }

	private void Awake()
	{
		TrapActivated = false;
	}

	private IEnumerator GrowCoroutine(float maxLength, float seconds)
	{
		float counter = 0f;
		while (counter < seconds)
		{
			float i = Mathf.Lerp(0f, maxLength, counter / seconds);
			beam.maxRange = i;
			counter += Time.deltaTime;
			yield return null;
			beamAnimator.speed = 0.01f;
		}
		beam.maxRange = maxLength;
		beamAnimator.speed = 1f;
	}

	private void StartGrow()
	{
		StartCoroutine(GrowCoroutine(beam.GetDistance() + 0.2f, maxSeconds));
	}

	[Button("Test Activate", ButtonSizes.Small)]
	private void Activate()
	{
		TrapActivated = true;
		trapAnimator.SetBool("ATTACK", value: true);
		PlayFallAudio();
		StartCoroutine(DeactivateAfterSeconds());
	}

	[Button("Test End", ButtonSizes.Small)]
	public void Deactivate()
	{
		StopFallAudio();
		TrapActivated = false;
		trapAnimator.SetBool("ATTACK", value: false);
	}

	private IEnumerator DeactivateAfterSeconds()
	{
		yield return new WaitForSeconds(timeActive);
		if (TrapActivated)
		{
			Use();
		}
	}

	public void StartFall()
	{
		beamAnimator.SetTrigger("FALL");
		beam.ActivateEndAnimation(active: true);
		StartGrow();
	}

	public void StopFall()
	{
		beamAnimator.SetTrigger("VANISH");
		beam.growSprite.gameObject.SetActive(value: false);
		beam.ActivateEndAnimation(active: false, applyChanges: true);
	}

	public void Use()
	{
		Debug.Log("USING TRAP");
		if (fallDelay > 0f)
		{
			Invoke("Toggle", fallDelay);
		}
		else
		{
			Toggle();
		}
	}

	private void Toggle()
	{
		if (TrapActivated)
		{
			Deactivate();
		}
		else
		{
			Activate();
		}
	}

	private void PlayFallAudio()
	{
		if (!string.IsNullOrEmpty(FallAudio))
		{
			DisposeFallAudioInstance();
			Core.Audio.PlayEventNoCatalog(ref _fallAudioEvent, FallAudio);
		}
	}

	private void StopFallAudio()
	{
		_fallAudioEvent.getParameter("End", out var instance);
		instance.setValue(1f);
	}

	private void DisposeFallAudioInstance()
	{
		if (_fallAudioEvent.isValid())
		{
			_fallAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_fallAudioEvent.release();
			_fallAudioEvent = default(EventInstance);
		}
	}

	private void OnDestroy()
	{
		DisposeFallAudioInstance();
	}
}
