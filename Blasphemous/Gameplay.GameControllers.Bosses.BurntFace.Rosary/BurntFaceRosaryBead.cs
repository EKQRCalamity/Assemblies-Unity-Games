using System.Collections;
using System.Collections.Generic;
using DamageEffect;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.Traps.Turrets;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace.Rosary;

public class BurntFaceRosaryBead : MonoBehaviour
{
	public enum ROSARY_BEAD_TYPE
	{
		BEAM,
		PROJECTILE
	}

	private BurntFaceRosaryPattern _currentPattern;

	private BurntFaceRosaryManager _manager;

	public int revolutionsRemaining;

	public float currentAngle;

	public Vector2 managerPosition;

	public List<BurntFaceRosaryAngles> angleSections;

	public StateMachine<BurntFaceRosaryBead> fsm;

	public State<BurntFaceRosaryBead> stInactive;

	public State<BurntFaceRosaryBead> stActive;

	public State<BurntFaceRosaryBead> stHidden;

	public State<BurntFaceRosaryBead> stCharging;

	public State<BurntFaceRosaryBead> stBeam;

	public State<BurntFaceRosaryBead> stProjectile;

	public State<BurntFaceRosaryBead> stDestroyed;

	public TileableBeamLauncher beamLauncher;

	public BasicTurret _turret;

	public AttackArea beamAttackArea;

	public DamageEffectScript damageEffect;

	public int hits;

	public int maxHits;

	public bool canBeDamaged;

	public float _warningTime = 0.5f;

	public ROSARY_BEAD_TYPE currentType;

	private const float MAX_CHARGE = 0.5f;

	private float chargeCounter;

	private void Awake()
	{
		stBeam = new BurntFaceRosaryBead_StBeam();
		stProjectile = new BurntFaceRosaryBead_StProjectile();
		stInactive = new BurntFaceRosaryBead_StInactive();
		stCharging = new BurntFaceRosaryBead_StCharging();
		stActive = new BurntFaceRosaryBead_StActive();
		stHidden = new BurntFaceRosaryBead_StHidden();
		stDestroyed = new BurntFaceRosaryBead_StDestroyed();
		fsm = new StateMachine<BurntFaceRosaryBead>(this, stInactive);
	}

	private void Update()
	{
		fsm.DoUpdate();
		managerPosition = _manager.center.position;
	}

	public void ForceDestroy()
	{
		fsm.ChangeState(stDestroyed);
	}

	public void Regenerate()
	{
		hits = maxHits;
		fsm.ChangeState(stInactive);
	}

	public void Hide()
	{
		if (!fsm.IsInState(stDestroyed))
		{
			fsm.ChangeState(stHidden);
		}
	}

	public void Show()
	{
		if (!fsm.IsInState(stDestroyed))
		{
			fsm.ChangeState(stInactive);
		}
	}

	public void SetLaserParentRotation(Vector2 d)
	{
		base.transform.GetChild(0).right = d;
	}

	public void Init(BurntFaceRosaryManager manager)
	{
		_manager = manager;
	}

	public bool IsDestroyed()
	{
		return fsm.IsInState(stDestroyed);
	}

	public void SetPattern(BurntFaceRosaryPattern pattern)
	{
		if (pattern.ID == "EMPTY")
		{
			if (!fsm.IsInState(stDestroyed))
			{
				fsm.ChangeState(stInactive);
			}
		}
		else if (!fsm.IsInState(stDestroyed))
		{
			fsm.ChangeState(stActive);
		}
		_currentPattern = pattern;
		currentType = pattern.beadType;
		angleSections = pattern.activeSections;
		if (pattern.beadType == ROSARY_BEAD_TYPE.PROJECTILE)
		{
			_turret.SetFireParameters(pattern.projectileSpeed, pattern.projectileFireRate);
		}
		else
		{
			_warningTime = pattern.beamWarningTime;
		}
	}

	public void UpdateAngle(float angleFromManager)
	{
		currentAngle = angleFromManager;
	}

	public void OnDestroyed()
	{
		_manager.OnBeadDestroyed(this);
	}

	public bool IsInsideSection(BurntFaceRosaryAngles section)
	{
		return currentAngle < section.endAngle && currentAngle > section.startAngle;
	}

	public bool IsInsideActiveAngle()
	{
		if (angleSections == null || angleSections.Count == 0)
		{
			return false;
		}
		foreach (BurntFaceRosaryAngles angleSection in angleSections)
		{
			if (IsInsideSection(angleSection))
			{
				return true;
			}
		}
		return false;
	}

	public void ActivateTurret(bool activate)
	{
		_turret.enabled = activate;
		_turret.ForceShoot();
	}

	public void ActivateBeam()
	{
		beamLauncher.gameObject.SetActive(value: true);
		beamLauncher.ActivateDelayedBeam(_warningTime, warningAnimation: true);
		Debug.Log("ACTIVATING BEAM");
	}

	public void DeactivateBeam()
	{
		Debug.Log("DEACTIVATING BEAM");
		beamLauncher.ActivateBeamAnimation(active: false);
		StartCoroutine(DelayedBeamDeactivation(0.3f));
	}

	private IEnumerator DelayedBeamDeactivation(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		beamLauncher.gameObject.SetActive(value: false);
	}

	public void ChangeToProjectile()
	{
		if (!fsm.IsInState(stDestroyed))
		{
			fsm.ChangeState(stProjectile);
		}
	}

	public void ChangeToBeam()
	{
		if (!fsm.IsInState(stDestroyed))
		{
			fsm.ChangeState(stBeam);
		}
	}

	public void UpdateChargeCounter()
	{
		chargeCounter += Time.deltaTime;
	}

	public bool IsCharged()
	{
		return currentType == ROSARY_BEAD_TYPE.PROJECTILE || chargeCounter > 0.5f;
	}

	public void ResetChargeCounter()
	{
		chargeCounter = 0f;
	}

	public void ChangeToCharging()
	{
		fsm.ChangeState(stCharging);
	}

	public void ChangeToInactive()
	{
		fsm.ChangeState(stInactive);
	}

	public void ChangeToActive()
	{
		fsm.ChangeState(stActive);
	}

	public void SetAnimatorDestroyed(bool dest)
	{
	}

	public void PlayShowAnimation()
	{
	}

	public void PlayHideAnimation()
	{
	}

	public void SetColliderActive(bool v)
	{
	}

	public void OnDrawGizmosSelected()
	{
		if (_manager == null)
		{
			return;
		}
		Gizmos.color = Color.magenta;
		if (angleSections == null)
		{
			return;
		}
		foreach (BurntFaceRosaryAngles angleSection in angleSections)
		{
			DrawGizmoSection(angleSection);
		}
	}

	private void DrawGizmoSection(BurntFaceRosaryAngles angle)
	{
		Quaternion quaternion = Quaternion.Euler(0f, 0f, angle.startAngle);
		Quaternion quaternion2 = Quaternion.Euler(0f, 0f, angle.endAngle);
		Vector2 right = Vector2.right;
		Vector2 vector = quaternion * right;
		Vector2 vector2 = quaternion2 * right;
		Gizmos.DrawSphere(managerPosition, 0.1f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(managerPosition, vector * 5f);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(managerPosition, vector2 * 5f);
		int num = Mathf.RoundToInt((angle.endAngle - angle.startAngle) / 2f);
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / (float)num;
			Gizmos.color = Color.Lerp(Color.cyan, Color.blue, t);
			Quaternion quaternion3 = Quaternion.Euler(0f, 0f, Mathf.Lerp(angle.startAngle, angle.endAngle, t));
			Gizmos.DrawRay(managerPosition, quaternion3 * Vector2.right * 4f);
		}
		Vector2 vector3 = Quaternion.Euler(0f, 0f, currentAngle) * Vector2.right;
		Color color = Color.red;
		if (IsInsideActiveAngle())
		{
			color = Color.green;
		}
		Gizmos.color = color;
		Gizmos.DrawRay(managerPosition, vector3 * 5f);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
