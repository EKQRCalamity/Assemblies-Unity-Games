using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class BasicTrap : MonoBehaviour, IActionable
{
	private enum TrapState
	{
		Idle,
		Moving
	}

	private enum TimeSyncType
	{
		Delay,
		Offset
	}

	private const string DAMAGE_START_EVENT = "DAMAGE_START";

	private const string DAMAGE_END_EVENT = "DAMAGE_END";

	private const string LOOP_END_EVENT = "LOOP_END";

	private const string TRAP_RETREAT_EVENT = "TRAP_RETREAT";

	private const string ANTICIPATION_STRING = "ANTICIPATION";

	private const string ANTICIPATION_END_EVENT = "ANTICIPATION_END";

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool looping = true;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool DamageWhileUnattacable = true;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private TimeSyncType syncType;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Range(0f, 60f)]
	private float activationDelay = 1f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Range(0f, 4f)]
	private float reactivationTime = 1f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private int damage = 1;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public bool startActive = true;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private DamageArea.DamageType damageType;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Range(0.5f, 2f)]
	private float animationSpeed = 1f;

	[SerializeField]
	[BoxGroup("Events", true, false, 0)]
	private string OnDamage;

	[SerializeField]
	[BoxGroup("Events", true, false, 0)]
	private string OnHurtModeEnter;

	[SerializeField]
	[BoxGroup("Events", true, false, 0)]
	private string OnHurtModeExit;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string activationSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string deactivationSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string anticipationEndSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string HitSound;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private AnimatorEvent animationEvent;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private TriggerSensor sensor;

	private List<Entity> targets = new List<Entity>();

	private TrapState state;

	private bool damaging;

	private EventInstance activationSoundInstnace;

	private EventInstance deactivationSoundInstance;

	private EventInstance anticipationEndSoundInstnace;

	private bool firstActivation = true;

	private bool waitingForActivationSound;

	private float anticipationClipSpeed = 1f;

	private float realReactivationTime;

	private bool anticipationAnimExists;

	private Camera mainCamera;

	public bool Locked { get; set; }

	private void Awake()
	{
		sensor.OnEntityEnter += OnTargetEnter;
		sensor.OnEntityExit += OnTargetExit;
		animationEvent.OnEventLaunched += OnEventLaunched;
		animator.speed = animationSpeed;
		if (!string.IsNullOrEmpty(activationSound))
		{
			activationSoundInstnace = Core.Audio.CreateEvent(activationSound);
		}
		if (!string.IsNullOrEmpty(deactivationSound))
		{
			deactivationSoundInstance = Core.Audio.CreateEvent(deactivationSound);
		}
		if (!string.IsNullOrEmpty(anticipationEndSound))
		{
			anticipationEndSoundInstnace = Core.Audio.CreateEvent(anticipationEndSound);
		}
		realReactivationTime = reactivationTime;
		AnimationClip animationClip = FindAnticipationClip();
		if (animationClip != null)
		{
			anticipationAnimExists = true;
			realReactivationTime = CalculateRealReactivationTime(animationClip);
		}
	}

	private void OnDestroy()
	{
		if ((bool)sensor)
		{
			sensor.OnEntityEnter -= OnTargetEnter;
			sensor.OnEntityExit -= OnTargetExit;
		}
		if ((bool)animationEvent)
		{
			animationEvent.OnEventLaunched -= OnEventLaunched;
		}
	}

	private IEnumerator Start()
	{
		mainCamera = Camera.main;
		if (startActive)
		{
			switch (syncType)
			{
			case TimeSyncType.Delay:
				yield return new WaitForSeconds(activationDelay);
				Use();
				break;
			case TimeSyncType.Offset:
			{
				float animationDuration = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
				float normalizedTime = (animationDuration - activationDelay) / animationDuration;
				animator.Play(0, 0, normalizedTime);
				Use();
				break;
			}
			}
		}
	}

	private void Update()
	{
		if (activationSoundInstnace.isValid())
		{
			Core.Audio.ModifyPanning(activationSoundInstnace, base.transform);
		}
		if (deactivationSoundInstance.isValid())
		{
			Core.Audio.ModifyPanning(deactivationSoundInstance, base.transform);
		}
		if (anticipationEndSoundInstnace.isValid())
		{
			Core.Audio.ModifyPanning(anticipationEndSoundInstnace, base.transform);
		}
		UpdateAnimatorSpeed();
		if (damaging && sensor.EntitiesInside)
		{
			if (Core.Logic.Penitent.Status.Unattacable && !DamageWhileUnattacable)
			{
				return;
			}
			DamageTargets();
			damaging = false;
		}
		if (waitingForActivationSound && spriteRenderer.IsVisibleFrom(mainCamera))
		{
			waitingForActivationSound = false;
			if (!string.IsNullOrEmpty(activationSound))
			{
				activationSoundInstnace.start();
			}
		}
	}

	private void UpdateAnimatorSpeed()
	{
		if (anticipationAnimExists && anticipationClipSpeed != 1f)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("ANTICIPATION"))
			{
				animator.speed = anticipationClipSpeed;
			}
			else
			{
				animator.speed = animationSpeed;
			}
		}
	}

	private void OnEventLaunched(string id)
	{
		switch (id)
		{
		case "DAMAGE_START":
			damaging = true;
			break;
		case "DAMAGE_END":
			damaging = false;
			break;
		case "TRAP_RETREAT":
			OnTrapRetreat();
			break;
		case "LOOP_END":
			OnUseFinish();
			break;
		case "ANTICIPATION_END":
			OnAnticipationEnd();
			break;
		}
	}

	private void OnTargetEnter(Entity entity)
	{
		if (!targets.Contains(entity))
		{
			targets.Add(entity);
		}
	}

	private void OnTargetExit(Entity entity)
	{
		if (targets.Contains(entity))
		{
			targets.Remove(entity);
		}
	}

	public void Use()
	{
		if (!Locked)
		{
			StartCoroutine(OnUse());
		}
	}

	private IEnumerator OnUse()
	{
		if (state == TrapState.Idle)
		{
			if (firstActivation && !startActive)
			{
				yield return new WaitForSeconds(activationDelay);
				firstActivation = false;
			}
			state = TrapState.Moving;
			NotifyAnimator(active: true);
			Locked = true;
			waitingForActivationSound = true;
		}
	}

	private void OnUseFinish()
	{
		state = TrapState.Idle;
		NotifyAnimator(active: false);
		Locked = false;
		waitingForActivationSound = false;
		if (looping)
		{
			Invoke("Use", realReactivationTime);
		}
	}

	private void NotifyAnimator(bool active)
	{
		if (anticipationAnimExists)
		{
			if (active)
			{
				animator.SetTrigger("ACTIVATE");
			}
			else
			{
				animator.SetTrigger("DEACTIVATE");
			}
		}
		else
		{
			animator.SetBool("ACTIVE", active);
		}
	}

	private AnimationClip FindAnticipationClip()
	{
		List<AnimationClip> list = new List<AnimationClip>(animator.runtimeAnimatorController.animationClips);
		return list.Find((AnimationClip x) => x.name.EndsWith("ANTICIPATION", StringComparison.InvariantCultureIgnoreCase));
	}

	private float CalculateRealReactivationTime(AnimationClip anticipationClip)
	{
		float result = 0f;
		float length = anticipationClip.length;
		if (reactivationTime - length > 0f)
		{
			result = reactivationTime - length;
		}
		else
		{
			anticipationClipSpeed = length / reactivationTime;
		}
		return result;
	}

	private void OnTrapRetreat()
	{
		if (spriteRenderer.IsVisibleFrom(mainCamera) && !string.IsNullOrEmpty(deactivationSound))
		{
			deactivationSoundInstance.start();
		}
	}

	private void OnAnticipationEnd()
	{
		if (spriteRenderer.IsVisibleFrom(mainCamera) && !string.IsNullOrEmpty(anticipationEndSound))
		{
			anticipationEndSoundInstnace.start();
		}
	}

	private void DamageTargets()
	{
		if (targets.Count != 0)
		{
			Hit hit = default(Hit);
			hit.AttackingEntity = base.gameObject;
			hit.Unnavoidable = true;
			hit.DamageAmount = damage;
			hit.DamageType = damageType;
			hit.Force = 0f;
			hit.HitSoundId = HitSound;
			Hit hit2 = hit;
			for (int i = 0; i < targets.Count; i++)
			{
				IDamageable componentInParent = targets[i].GetComponentInParent<IDamageable>();
				componentInParent.Damage(hit2);
			}
		}
	}
}
