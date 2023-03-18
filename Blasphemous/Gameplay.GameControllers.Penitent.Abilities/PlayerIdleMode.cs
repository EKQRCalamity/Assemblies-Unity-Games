using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.InputSystem;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class PlayerIdleMode : Trait
{
	private enum PlayerMode
	{
		Play,
		Idle
	}

	[Range(0f, 60f)]
	public float timeToIdleMode = 15f;

	private float _currentIdleTime;

	private PlatformCharacterInput _playerInput;

	private PlayerMode _currentPlayerMode;

	private bool IsDemakeMode => Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);

	private bool InputReceived
	{
		get
		{
			bool flag = Math.Abs(_playerInput.FHorAxis) > float.Epsilon;
			bool flag2 = Math.Abs(_playerInput.FVerAxis) > float.Epsilon;
			return _playerInput.Rewired.GetAnyButton() || flag || flag2;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_playerInput = Core.Logic.Penitent.PlatformCharacterInput;
		_currentPlayerMode = PlayerMode.Play;
		base.EntityOwner.OnDamaged += OnDamaged;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsDemakeMode)
		{
			return;
		}
		switch (_currentPlayerMode)
		{
		case PlayerMode.Play:
			if (base.EntityOwner.Status.IsIdle && !Core.Input.InputBlocked)
			{
				_currentIdleTime += Time.deltaTime;
				if (_currentIdleTime >= timeToIdleMode)
				{
					SetToIdleMode();
				}
			}
			else
			{
				ResetIdleTimer();
			}
			break;
		case PlayerMode.Idle:
			if (InputReceived)
			{
				SetToPlayMode();
			}
			break;
		}
	}

	private void SetToIdleMode(Action callBack = null)
	{
		_currentPlayerMode = PlayerMode.Idle;
		base.EntityOwner.Animator.SetBool("IS_IDLE_MODE", value: true);
		Core.Logic.Penitent.Audio.PlayIdleModeBlood();
		callBack?.Invoke();
	}

	private void SetToPlayMode(Action callBack = null)
	{
		_currentPlayerMode = PlayerMode.Play;
		base.EntityOwner.Animator.SetBool("IS_IDLE_MODE", value: false);
		Core.Logic.Penitent.Audio.StopIdleModeBlood();
		ResetIdleTimer();
		callBack?.Invoke();
	}

	private void ResetIdleTimer()
	{
		if (_currentIdleTime > 0f)
		{
			_currentIdleTime = 0f;
		}
	}

	private void SetAnimation(int animation)
	{
		UnityEngine.Animator animator = base.EntityOwner.Animator;
		if ((bool)animator)
		{
			animator.Play(animation);
		}
	}

	private void OnDamaged()
	{
		SetToPlayMode();
	}

	private void OnDestroy()
	{
		base.EntityOwner.OnDamaged -= OnDamaged;
	}
}
