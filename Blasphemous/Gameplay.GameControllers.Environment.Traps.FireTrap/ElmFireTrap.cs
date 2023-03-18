using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

public class ElmFireTrap : MonoBehaviour
{
	public enum LinkType
	{
		Core,
		Joint,
		Static
	}

	public Core.SimpleEvent OnCycleFinished;

	public Core.SimpleEventParam OnChargeStart;

	public Core.SimpleEventParam OnLightningCast;

	[FoldoutGroup("Target Settings", 0)]
	public LinkType linkType;

	[FoldoutGroup("Target Settings", 0)]
	[ShowIf("linkType", LinkType.Static, true)]
	public float maxTimeActive;

	[FoldoutGroup("Target Settings", 0)]
	[ShowIf("linkType", LinkType.Static, true)]
	public float maxTimeInactive;

	[FoldoutGroup("Target Settings", 0)]
	[HideIf("linkType", LinkType.Static, true)]
	public ElmFireTrap target;

	[FoldoutGroup("Target Settings", 0)]
	[HideIf("linkType", LinkType.Static, true)]
	public bool hasMoreTargets;

	[FoldoutGroup("Target Settings", 0)]
	[ShowIf("hasMoreTargets", true)]
	public List<ElmFireTrap> additionalTargets;

	[FoldoutGroup("Lightning Settings", 0)]
	[ShowIf("linkType", LinkType.Core, true)]
	public float lightningCycleCooldown;

	[FoldoutGroup("Lightning Settings", 0)]
	[HideIf("linkType", LinkType.Static, true)]
	public float chargingTime;

	[FoldoutGroup("Lightning Settings", 0)]
	[HideIf("linkType", LinkType.Static, true)]
	public float targetLightningChargeTimeout = 1.85f;

	[BoxGroup("Debug", true, false, 0)]
	public bool drawGizmos;

	public TileableBeamLauncher LightningPrefab;

	private ElmFireTrapCore _trapCore;

	private float currentTimeActive;

	private float currentTimeInactive;

	private BeamAttack lightningBeamAttack;

	private List<TileableBeamLauncher> ChargedLightnings = new List<TileableBeamLauncher>();

	[HideInInspector]
	public Collider2D Collider { get; set; }

	public Animator Animator { get; set; }

	public ElmFireTrapAttack Attack { get; set; }

	private void Awake()
	{
		if (linkType == LinkType.Core)
		{
			_trapCore = new ElmFireTrapCore(this);
		}
		Animator = GetComponentInChildren<Animator>();
		Collider = GetComponentInChildren<CircleCollider2D>();
		Attack = GetComponent<ElmFireTrapAttack>();
	}

	private void Start()
	{
		Animator.Play("Burning", 0, Random.value);
		if ((bool)LightningPrefab)
		{
			PoolManager.Instance.CreatePool(LightningPrefab.gameObject, 1);
		}
	}

	private void Update()
	{
		if (_trapCore != null)
		{
			_trapCore.Update();
		}
		if (linkType != LinkType.Static)
		{
			return;
		}
		if (Collider.enabled)
		{
			currentTimeActive += Time.deltaTime;
			if (currentTimeActive > maxTimeActive)
			{
				currentTimeActive = 0f;
				currentTimeInactive = 0f;
				Animator.SetTrigger("HIDE");
			}
		}
		else
		{
			currentTimeInactive += Time.deltaTime;
			if (currentTimeInactive > maxTimeInactive)
			{
				currentTimeActive = 0f;
				currentTimeInactive = 0f;
				Animator.SetTrigger("SHOW");
			}
		}
	}

	public void SetCurrentCycleCooldownToMax()
	{
		_trapCore.SetCurrentCycleCooldownToMax();
	}

	public void AnimEvent_ActivateCollider()
	{
		Collider.enabled = true;
	}

	public void AnimEvent_DeactivateCollider()
	{
		Collider.enabled = false;
	}

	public void ChargeLightnings()
	{
		if (!target)
		{
			if (OnCycleFinished != null)
			{
				OnCycleFinished();
			}
			return;
		}
		TileableBeamLauncher tileableBeam = GetTileableBeam(target);
		if (!tileableBeam)
		{
			return;
		}
		ChargedLightnings.Add(tileableBeam);
		float maxRange = Vector2.Distance(target.transform.position, base.transform.position);
		tileableBeam.maxRange = maxRange;
		tileableBeam.ActivateDelayedBeam(0f, warningAnimation: true);
		if (hasMoreTargets)
		{
			foreach (ElmFireTrap additionalTarget in additionalTargets)
			{
				tileableBeam = GetTileableBeam(additionalTarget);
				if (!tileableBeam)
				{
					return;
				}
				ChargedLightnings.Add(tileableBeam);
				maxRange = Vector2.Distance(additionalTarget.transform.position, base.transform.position);
				tileableBeam.maxRange = maxRange;
				tileableBeam.ActivateDelayedBeam(0f, warningAnimation: true);
			}
		}
		StartCastLightningsSequence();
	}

	private TileableBeamLauncher GetTileableBeam(ElmFireTrap fireTrap)
	{
		Vector3 position = fireTrap.transform.position;
		Vector3 position2 = (position + base.transform.position) / 2f;
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(LightningPrefab.gameObject, position2, GetLightningRotation(position));
		lightningBeamAttack = objectInstance.GameObject.GetComponentInChildren<BeamAttack>();
		return objectInstance.GameObject.GetComponent<TileableBeamLauncher>();
	}

	public void SetDamage(float damage)
	{
		if ((bool)lightningBeamAttack)
		{
			lightningBeamAttack.lightningHit.DamageAmount = damage;
		}
	}

	private Quaternion GetLightningRotation(Vector3 targetPos)
	{
		Vector3 vector = targetPos - base.transform.position;
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		return Quaternion.Euler(0f, 0f, z);
	}

	private void StartCastLightningsSequence()
	{
		if (!target)
		{
			return;
		}
		if (OnChargeStart != null)
		{
			OnChargeStart(chargingTime);
		}
		DOTween.Sequence().SetDelay(chargingTime).OnComplete(delegate
		{
			foreach (TileableBeamLauncher chargedLightning in ChargedLightnings)
			{
				chargedLightning.TriggerBeamBodyAnim();
			}
			ChargeTargetLightningsSequence();
			ChargedLightnings.Clear();
			if (OnLightningCast != null)
			{
				OnLightningCast(target.gameObject.transform.position);
			}
		});
	}

	private void ChargeTargetLightningsSequence()
	{
		DOTween.Sequence().SetDelay(targetLightningChargeTimeout).OnComplete(ChargeTargetsLightnings);
	}

	private void ChargeTargetsLightnings()
	{
		if (!target)
		{
			return;
		}
		target.ChargeLightnings();
		if (!hasMoreTargets)
		{
			return;
		}
		foreach (ElmFireTrap additionalTarget in additionalTargets)
		{
			additionalTarget.ChargeLightnings();
		}
	}

	public void ResetTrapCycle()
	{
		_trapCore.ResetCycle();
	}
}
