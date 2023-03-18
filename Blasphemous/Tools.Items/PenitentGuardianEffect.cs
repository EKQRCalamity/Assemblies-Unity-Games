using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Tools.Items;

public class PenitentGuardianEffect : ObjectEffect
{
	private GameObject _penitentGuardian;

	public GameObject PenitentGuardianPrefab;

	public ItemTemporalEffect InvulnerableEffect;

	public float YOffset = 1f;

	private Penitent _owner;

	private bool _showLady;

	protected override bool OnApplyEffect()
	{
		if (!InvulnerableEffect.IsApplied)
		{
			return base.OnApplyEffect();
		}
		InstantiateGuardian();
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		return base.OnApplyEffect();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		_owner = penitent;
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (InvulnerableEffect == null)
		{
			Debug.LogError("Invulnerable effect must be set!");
		}
	}

	private void Update()
	{
		if (!InvulnerableEffect || !InvulnerableEffect.IsApplied)
		{
			if (_showLady)
			{
				_showLady = false;
				Core.Logic.Penitent.DamageArea.PrayerProtectionEnabled = false;
				if (_owner.Status.Invulnerable)
				{
					_owner.Status.Invulnerable = false;
				}
			}
		}
		else if (!_owner.Status.Invulnerable)
		{
			_showLady = true;
			Core.Logic.Penitent.DamageArea.PrayerProtectionEnabled = true;
			_owner.Status.Invulnerable = true;
		}
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
	}

	public void InstantiateGuardian()
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		position.y += YOffset;
		if (_penitentGuardian == null)
		{
			if (PenitentGuardianPrefab != null)
			{
				_penitentGuardian = Object.Instantiate(PenitentGuardianPrefab, position, Quaternion.identity);
				PenitentGuardian component = _penitentGuardian.GetComponent<PenitentGuardian>();
				component.SetOrientation(currentHit);
			}
			return;
		}
		_penitentGuardian.SetActive(value: true);
		PenitentGuardian component2 = _penitentGuardian.GetComponent<PenitentGuardian>();
		if (!component2.IsTriggered)
		{
			component2.SetOrientation(currentHit);
			_penitentGuardian.transform.position = position;
		}
	}

	public void DisableGuardian()
	{
		if (_penitentGuardian.activeSelf)
		{
			_penitentGuardian.SetActive(value: false);
		}
	}
}
