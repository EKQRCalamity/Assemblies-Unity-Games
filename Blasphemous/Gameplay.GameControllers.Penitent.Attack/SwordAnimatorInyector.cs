using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

[RequireComponent(typeof(UnityEngine.Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SwordAnimatorInyector : MonoBehaviour
{
	public Entity OwnerEntity;

	public UnityEngine.Animator SwordAnimator;

	public SpriteRenderer SpriteRenderer;

	public const int CrouchAttack = 1;

	public const int BasicAttackType1 = 2;

	public const int BasicAttackType2 = 3;

	public const int ComboAttack = 4;

	public const int AirAttack1 = 5;

	public const int AirAttack2 = 6;

	public const int GroundUpward = 7;

	public const int AirUpward = 8;

	private int _lastEntityAnimatorState;

	private int _currentEntityAnimatorState;

	private int _defaultStateHashName;

	private int _attackStateHashName;

	private bool IsXFlip { get; set; }

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	private void Start()
	{
		if (SwordAnimator == null)
		{
			Debug.LogError("A Sprite Animator is needed");
		}
		if (OwnerEntity == null)
		{
			Debug.LogError("A Owner is needed");
		}
		SpriteRenderer = GetComponent<SpriteRenderer>();
		_lastEntityAnimatorState = OwnerEntity.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
		_defaultStateHashName = UnityEngine.Animator.StringToHash("Default");
		_attackStateHashName = UnityEngine.Animator.StringToHash("Attack");
	}

	private void Update()
	{
		SpriteRendererFlip();
		_currentEntityAnimatorState = OwnerEntity.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
		if (_lastEntityAnimatorState != _currentEntityAnimatorState)
		{
			_lastEntityAnimatorState = _currentEntityAnimatorState;
			if (OwnerEntity.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _attackStateHashName && !IsDemakeMode && SwordAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != _defaultStateHashName)
			{
				SwordAnimator.Play(_defaultStateHashName);
			}
		}
	}

	public void SetSlashAnimation(PenitentSword.SwordSlash slashType)
	{
		switch (slashType.Type)
		{
		case PenitentSword.AttackType.Crouch:
			SetAnimatorParameters(1, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.Basic1:
			SetAnimatorParameters(2, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.Basic2:
			SetAnimatorParameters(3, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.Combo:
			SetAnimatorParameters(4, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.Air1:
			SetAnimatorParameters(5, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.Air2:
			SetAnimatorParameters(6, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.AirUpward:
			SetAnimatorParameters(8, slashType.Level, slashType.Color);
			break;
		case PenitentSword.AttackType.GroundUpward:
			SetAnimatorParameters(7, slashType.Level, slashType.Color);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetAnimatorParameters(int type, int level, int color)
	{
		if (!(SwordAnimator == null))
		{
			SwordAnimator.SetInteger("TYPE", type);
			SwordAnimator.SetInteger("LEVEL", level);
			SwordAnimator.SetInteger("COLOR", color);
		}
	}

	public void ResetParameters()
	{
		if (!(SwordAnimator == null))
		{
			SwordAnimator.SetInteger("LEVEL", 0);
			SwordAnimator.SetInteger("TYPE", 0);
			SwordAnimator.SetInteger("COLOR", 0);
		}
	}

	public void SetAnimatorSpeed(float speed)
	{
		float speed2 = Mathf.Clamp(speed, 1f, 2f);
		SwordAnimator.speed = speed2;
	}

	public void PlayAttackDesiredTime(int attackLevel, float desiredTime, PenitentSword.AttackColor color, string attackLabel = "Basic1_Lv")
	{
		string text = string.Empty;
		if (attackLevel > 1)
		{
			text = color switch
			{
				PenitentSword.AttackColor.Default => text + string.Empty, 
				PenitentSword.AttackColor.Red => text + "_Red", 
				_ => text + string.Empty, 
			};
		}
		string stateName = attackLabel + attackLevel + text;
		SwordAnimator.Play(stateName, 0, desiredTime);
		ResetParameters();
	}

	private void SpriteRendererFlip()
	{
		if (OwnerEntity.Status.Orientation == EntityOrientation.Left && !IsXFlip)
		{
			IsXFlip = true;
			SpriteRenderer.flipX = true;
		}
		else if (OwnerEntity.Status.Orientation == EntityOrientation.Right && IsXFlip)
		{
			IsXFlip = false;
			SpriteRenderer.flipX = false;
		}
	}

	public int GetColorValue(PenitentSword.AttackColor attackColor)
	{
		int num = 0;
		return attackColor switch
		{
			PenitentSword.AttackColor.Default => 0, 
			PenitentSword.AttackColor.Red => 1, 
			_ => 0, 
		};
	}
}
