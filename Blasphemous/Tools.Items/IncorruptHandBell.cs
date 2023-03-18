using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using Tools.Level.Actionables;
using UnityEngine;

namespace Tools.Items;

public class IncorruptHandBell : ObjectEffect
{
	private const float UPDATE_RATE = 3f;

	private const float InitialWarningDelay = 3f;

	private float InitialWarningDelayCounter;

	private const float INTENSITY_LEVELS = 5f;

	private const string AUDIO_INTENSITY_PARAM = "Distance";

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string audioId;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float interactionDistance;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private IncorruptHandConfig[] intensityConfiguration = new IncorruptHandConfig[5];

	[SerializeField]
	[BoxGroup("Debug Information", true, false, 0)]
	[ReadOnly]
	private int audioIntensity;

	[SerializeField]
	[BoxGroup("Debug Information", true, false, 0)]
	[ReadOnly]
	private Color haloColor;

	[SerializeField]
	[BoxGroup("Debug Information", true, false, 0)]
	[ReadOnly]
	private float haloDuration;

	private List<BreakableWall> secrets;

	private BreakableWall closestTarget;

	private EventInstance instance;

	private float UpdateLapse { get; set; }

	private bool SecretsInRange => closestTarget != null && ClosestSecretDistance <= interactionDistance;

	private float ClosestSecretDistance
	{
		get
		{
			if (closestTarget != null)
			{
				return Vector2.Distance(closestTarget.transform.position, Core.Logic.Penitent.transform.position);
			}
			return -1f;
		}
	}

	private float NormalizedProgress => ClosestSecretDistance / interactionDistance;

	private PLAYBACK_STATE PlaybackState
	{
		get
		{
			instance.getPlaybackState(out var state);
			return state;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		HiddenArea.OnUse = (Core.SimpleEvent)Delegate.Combine(HiddenArea.OnUse, new Core.SimpleEvent(OnUse));
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
	}

	private void OnBeforeLevelLoad(Framework.FrameworkCore.Level oldlevel, Framework.FrameworkCore.Level newlevel)
	{
		InitialWarningDelayCounter = 0f;
		if (secrets != null && secrets.Count > 0)
		{
			secrets.Clear();
		}
	}

	private void OnUse()
	{
		if (secrets != null && secrets.Count > 0 && !(closestTarget == null))
		{
			secrets.Remove(closestTarget);
			closestTarget = null;
		}
	}

	protected override bool OnApplyEffect()
	{
		CreateAudioInstance();
		secrets = UnityEngine.Object.FindObjectsOfType<BreakableWall>().ToList().FindAll((BreakableWall x) => !x.Destroyed);
		UpdateLapse = 3f;
		return true;
	}

	protected override void OnRemoveEffect()
	{
		instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		float deltaTime = Time.deltaTime;
		if (secrets != null && secrets.Count > 0)
		{
			InitialWarningDelayCounter += deltaTime;
		}
		UpdateLapse += deltaTime;
		if (UpdateLapse > 3f)
		{
			UpdateLapse = 0f;
			TimedUpdate();
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		HiddenArea.OnUse = (Core.SimpleEvent)Delegate.Remove(HiddenArea.OnUse, new Core.SimpleEvent(OnUse));
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
		if (instance.isValid())
		{
			instance.release();
		}
	}

	private void TimedUpdate()
	{
		UpdateClosestTarget();
		UpdateIntensity();
		UpdateItemLogic();
	}

	private void UpdateItemLogic()
	{
		if (!(InitialWarningDelayCounter < 3f) && SecretsInRange && !Core.Logic.Penitent.Status.Dead && closestTarget != null)
		{
			instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			instance.start();
			Core.UI.Glow.color = haloColor;
			Core.UI.Glow.Show(haloDuration);
		}
	}

	private void UpdateClosestTarget()
	{
		Penitent penitent = Core.Logic.Penitent;
		if (secrets == null || secrets.Count == 0 || penitent == null)
		{
			return;
		}
		Vector3 position = penitent.transform.position;
		foreach (BreakableWall secret in secrets)
		{
			if (closestTarget == null)
			{
				closestTarget = secret;
			}
			float num = Vector2.Distance(secret.transform.position, position);
			float num2 = Vector2.Distance(closestTarget.transform.position, position);
			if (num < num2)
			{
				closestTarget = secret;
			}
		}
	}

	private void UpdateIntensity()
	{
		audioIntensity = Mathf.CeilToInt(5f - NormalizedProgress * 5f);
		instance.setParameterValue("Distance", audioIntensity);
		int num = audioIntensity - 1;
		if (num >= 0 && num < intensityConfiguration.Length)
		{
			haloColor = new Color(1f, 1f, 1f, intensityConfiguration[num].haloTransparency);
			haloDuration = intensityConfiguration[num].haloDuration;
		}
	}

	public void CreateAudioInstance()
	{
		if (!instance.isValid())
		{
			instance = Core.Audio.CreateEvent(audioId);
		}
	}
}
