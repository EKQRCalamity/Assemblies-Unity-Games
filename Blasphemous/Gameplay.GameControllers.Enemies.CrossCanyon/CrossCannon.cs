using System;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.GameControllers.Enemies.CrossCanyon;

public class CrossCannon : MonoBehaviour
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	[Serializable]
	public struct Cannon
	{
		public Direction Direction;

		public BossStraightProjectileAttack ProjectileAttack;
	}

	public float ShotInterval;

	private float _currentShotInterval;

	[FormerlySerializedAs("Canyons")]
	public Cannon[] Cannons;

	protected Entity Owner { get; private set; }

	private void Start()
	{
		_currentShotInterval = ShotInterval;
		Owner = GetComponentInParent<Entity>();
		SetCannonsDamage();
		if (!(Owner != null))
		{
			Debug.LogError("This component requires an entity component.");
			base.enabled = false;
			Owner.OnDeath += OnDeath;
		}
	}

	private void Update()
	{
		_currentShotInterval -= Time.deltaTime;
		if (_currentShotInterval <= 0f && !Owner.Status.Dead)
		{
			_currentShotInterval = ShotInterval;
			ShootCannons();
		}
	}

	private void Rotate()
	{
		Quaternion quaternion = Quaternion.Euler(0f, 0f, -45f);
		base.transform.rotation *= quaternion;
	}

	private void ShootCannons()
	{
		Cannon[] cannons = Cannons;
		for (int i = 0; i < cannons.Length; i++)
		{
			Cannon cannon = cannons[i];
			Vector2 shotDirection = GetShotDirection(cannon.Direction);
			cannon.ProjectileAttack.Shoot(shotDirection);
		}
		Rotate();
	}

	private void SetCannonsDamage()
	{
		IProjectileAttack[] componentsInChildren = GetComponentsInChildren<IProjectileAttack>();
		IProjectileAttack[] array = componentsInChildren;
		foreach (IProjectileAttack projectileAttack in array)
		{
			projectileAttack.SetProjectileWeaponDamage((int)Owner.Stats.Strength.Final);
		}
	}

	private Vector2 GetShotDirection(Direction dir)
	{
		Vector2 result = Vector2.right;
		switch (dir)
		{
		case Direction.Up:
			result = base.transform.up;
			break;
		case Direction.Down:
			result = -base.transform.up;
			break;
		case Direction.Left:
			result = -base.transform.right;
			break;
		case Direction.Right:
			result = base.transform.right;
			break;
		}
		return result;
	}

	private void OnDeath()
	{
		Owner.OnDeath -= OnDeath;
		_currentShotInterval = ShotInterval;
		base.enabled = false;
	}
}
