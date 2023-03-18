using System.Collections.Generic;
using CreativeSpore.SmartColliders;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class LungeAttack : Ability
{
	public Core.SimpleEvent OnLungeAttackStart;

	private Penitent _penitent;

	public Dash.MoveSetting LungeMoveSetting;

	[BoxGroup("Lunge Movement", true, false, 0)]
	public float LungeLapse = 0.5f;

	[BoxGroup("Lunge Movement", true, false, 0)]
	public float LungeLapse2 = 0.6f;

	[BoxGroup("Lunge Movement", true, false, 0)]
	public float LungeLapse3 = 0.7f;

	public float LungeDrag;

	public float LungeMaxWalkingSpeed;

	public float LungeMaxWalkingSpeed2;

	public float LungeMaxWalkingSpeed3;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string LungeEnemyHitFx;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string LungeMovementFxLevel1;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string LungeMovementFxLevel2;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string LungeMovementFxLevel3;

	private EventInstance _lungeMovementFx;

	[SerializeField]
	[BoxGroup("Damage Factor", true, false, 0)]
	public float DamageFactor1;

	[SerializeField]
	[BoxGroup("Damage Factor", true, false, 0)]
	public float DamageFactor2;

	[SerializeField]
	[BoxGroup("Damage Factor", true, false, 0)]
	public float DamageFactor3;

	private PlatformCharacterController _playerController;

	private float _currentLungeLapse;

	private bool _stopMovementComplete;

	private AttackArea _attackArea;

	private Hit _lungeHit;

	private bool _newEnemyHit;

	private List<GameObject> _hitEntities;

	[SerializeField]
	[Header("BoxGroup")]
	public float _myWidth;

	[SerializeField]
	[Header("BoxGroup")]
	public float _myHeight;

	private RaycastHit2D[] _bottomHits;

	public LayerMask GroundLayerMask;

	public readonly int LungAttackAnim = UnityEngine.Animator.StringToHash("LungeAttack");

	public readonly int LungAttackAnimLv2 = UnityEngine.Animator.StringToHash("LungeAttack_Lv2");

	public readonly int LungAttackAnimLv3 = UnityEngine.Animator.StringToHash("LungeAttack_Lv3");

	public bool CanHit { get; set; }

	public bool IsAvailable => CanExecuteSkilledAbility() && base.HasEnoughFervour;

	private string GetLungeFxKeyByLevel
	{
		get
		{
			if (base.LastUnlockedSkillId.IsNullOrWhitespace())
			{
				return null;
			}
			string empty = string.Empty;
			return base.LastUnlockedSkillId switch
			{
				"LUNGE_1" => LungeMovementFxLevel1, 
				"LUNGE_2" => LungeMovementFxLevel2, 
				"LUNGE_3" => LungeMovementFxLevel3, 
				_ => LungeMovementFxLevel1, 
			};
		}
	}

	private float GetDamageFactorByLevel()
	{
		if (base.LastUnlockedSkillId.IsNullOrWhitespace())
		{
			return 1f;
		}
		return base.LastUnlockedSkillId switch
		{
			"LUNGE_1" => DamageFactor1, 
			"LUNGE_2" => DamageFactor2, 
			"LUNGE_3" => DamageFactor3, 
			_ => DamageFactor1, 
		};
	}

	public float GetLungeSpeedByLevel()
	{
		if (base.LastUnlockedSkillId.IsNullOrWhitespace())
		{
			return 1f;
		}
		return base.LastUnlockedSkillId switch
		{
			"LUNGE_1" => LungeMaxWalkingSpeed, 
			"LUNGE_2" => LungeMaxWalkingSpeed2, 
			"LUNGE_3" => LungeMaxWalkingSpeed3, 
			_ => LungeMaxWalkingSpeed, 
		};
	}

	public float GetLungeLapseByLevel()
	{
		if (base.LastUnlockedSkillId.IsNullOrWhitespace())
		{
			return 1f;
		}
		return base.LastUnlockedSkillId switch
		{
			"LUNGE_1" => LungeLapse, 
			"LUNGE_2" => LungeLapse2, 
			"LUNGE_3" => LungeLapse3, 
			_ => LungeLapse, 
		};
	}

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = (Penitent)base.EntityOwner;
		_playerController = _penitent.PlatformCharacterController;
		_attackArea = _penitent.GetComponentInChildren<AttackArea>();
		_attackArea.OnEnter += AttackAreaOnEnter;
		_attackArea.OnStay += AttackAreaOnStay;
		LungeMoveSetting = new Dash.MoveSetting(LungeDrag, LungeMaxWalkingSpeed);
		_lungeHit = new Hit
		{
			AttackingEntity = _penitent.gameObject,
			DamageType = DamageArea.DamageType.Heavy,
			HitSoundId = LungeEnemyHitFx
		};
		_hitEntities = new List<GameObject>();
		_bottomHits = new RaycastHit2D[2];
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Casting)
		{
			return;
		}
		bool flag = IsGrounded();
		if (!flag)
		{
			_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
		}
		_currentLungeLapse += Time.deltaTime;
		if (_currentLungeLapse <= GetLungeLapseByLevel())
		{
			if (flag)
			{
				AddLungeForce();
			}
		}
		else
		{
			StopLungeForce();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		if (OnLungeAttackStart != null)
		{
			OnLungeAttackStart();
		}
		base.LastUnlockedSkillId = GetLastUnlockedSkill().id;
		Core.Audio.EventOneShotPanned(GetLungeFxKeyByLevel, base.transform.position, out _lungeMovementFx);
		_playerController.WalkingDrag = LungeDrag;
		_playerController.MaxWalkingSpeed = GetLungeSpeedByLevel();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		_currentLungeLapse = 0f;
		CanHit = false;
		_playerController.WalkingDrag = _penitent.Dash.DefaultMoveSetting.Drag;
		_playerController.MaxWalkingSpeed = _penitent.Dash.DefaultMoveSetting.Speed;
		if (_lungeMovementFx.isValid())
		{
			_lungeMovementFx.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_lungeMovementFx.release();
		}
		ClearHitEntityList();
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam e)
	{
		_newEnemyHit = true;
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		if (CanHit && base.Casting && _newEnemyHit)
		{
			GameObject gameObject = e.Collider2DArg.gameObject;
			if (!_hitEntities.Contains(gameObject))
			{
				_newEnemyHit = false;
				_hitEntities.Add(gameObject);
				IDamageable componentInParent = gameObject.GetComponentInParent<IDamageable>();
				AttackDamageableEntity(componentInParent);
			}
		}
	}

	private void ClearHitEntityList()
	{
		if (_hitEntities.Count > 0)
		{
			_hitEntities.Clear();
		}
	}

	private void AttackDamageableEntity(IDamageable damageable)
	{
		if (damageable == null)
		{
			return;
		}
		float damageFactorByLevel = GetDamageFactorByLevel();
		_lungeHit.DamageAmount = _penitent.Stats.Strength.Final * damageFactorByLevel;
		Enemy enemy = damageable as Enemy;
		if (enemy != null)
		{
			if (enemy.IsVulnerable)
			{
				enemy.GetStun(_lungeHit);
			}
			else
			{
				_penitent.PenitentAttack.CurrentPenitentWeapon.Attack(_lungeHit);
			}
		}
		else
		{
			_penitent.PenitentAttack.CurrentPenitentWeapon.Attack(_lungeHit);
		}
	}

	private bool IsGrounded()
	{
		bool flag = false;
		if (base.EntityOwner.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position - (0.75f * _myWidth * (Vector2)base.transform.right + Vector2.up * (_myHeight * 4f));
			Debug.DrawLine(vector, vector - Vector2.up * 1f, Color.yellow);
			return Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * 1f, _bottomHits, GroundLayerMask) > 0;
		}
		Vector2 vector2 = (Vector2)base.transform.position + (0.75f * _myWidth * (Vector2)base.transform.right - Vector2.up * (_myHeight * 4f));
		Debug.DrawLine(vector2, vector2 - Vector2.up * 1f, Color.yellow);
		return Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * 1f, _bottomHits, GroundLayerMask) > 0;
	}

	private void AddLungeForce()
	{
		if (_stopMovementComplete)
		{
			_stopMovementComplete = !_stopMovementComplete;
		}
		if (_penitent.Status.IsGrounded && !_penitent.HasFlag("FRONT_BLOCKED"))
		{
			_playerController.WalkingDrag = LungeMoveSetting.Drag;
			_playerController.MaxWalkingSpeed = GetLungeSpeedByLevel();
			_playerController.SetActionState((_penitent.Status.Orientation != EntityOrientation.Left) ? eControllerActions.Right : eControllerActions.Left, value: true);
		}
	}

	private void StopLungeForce()
	{
		if (!_stopMovementComplete)
		{
			_stopMovementComplete = true;
			DOTween.To(() => _playerController.WalkingDrag, delegate(float x)
			{
				_playerController.WalkingDrag = x;
			}, _penitent.Dash.DefaultMoveSetting.Drag, 1f);
			DOTween.To(() => _playerController.MaxWalkingSpeed, delegate(float x)
			{
				_playerController.MaxWalkingSpeed = x;
			}, _penitent.Dash.DefaultMoveSetting.Speed, 1f);
		}
	}

	public void PlayLungeAnimByLevelReached()
	{
		if (!base.LastUnlockedSkillId.IsNullOrWhitespace())
		{
			int stateNameHash = base.LastUnlockedSkillId switch
			{
				"LUNGE_1" => LungAttackAnim, 
				"LUNGE_2" => LungAttackAnimLv2, 
				"LUNGE_3" => LungAttackAnimLv3, 
				_ => LungAttackAnim, 
			};
			base.EntityOwner.Animator.Play(stateNameHash);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_attackArea)
		{
			_attackArea.OnEnter -= AttackAreaOnEnter;
			_attackArea.OnStay -= AttackAreaOnStay;
		}
	}
}
