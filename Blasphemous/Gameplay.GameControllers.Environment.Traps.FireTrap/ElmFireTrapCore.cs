using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

public class ElmFireTrapCore
{
	private ElmFireTrap _fireTrap;

	private float _currentCycleCooldown;

	private bool _isLightningCycleRunning;

	public bool IsActive { get; set; }

	public ElmFireTrapCore(ElmFireTrap fireTrap)
	{
		_fireTrap = fireTrap;
	}

	public void SetCurrentCycleCooldownToMax()
	{
		_currentCycleCooldown = _fireTrap.lightningCycleCooldown;
	}

	public void Update()
	{
		if (!_isLightningCycleRunning)
		{
			_currentCycleCooldown += Time.deltaTime;
			if (_currentCycleCooldown >= _fireTrap.lightningCycleCooldown)
			{
				_currentCycleCooldown = 0f;
				StartLightningCycle();
			}
		}
	}

	public void StartLightningCycle()
	{
		_isLightningCycleRunning = true;
		_fireTrap.ChargeLightnings();
	}

	public void ResetCycle()
	{
		_isLightningCycleRunning = false;
	}
}
