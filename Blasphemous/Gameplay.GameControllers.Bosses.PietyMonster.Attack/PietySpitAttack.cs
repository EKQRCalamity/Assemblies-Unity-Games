using System.Collections.Generic;
using Gameplay.GameControllers.Bosses.PietyMonster.IA;
using Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietySpitAttack : EnemyAttack
{
	private PietyMonster _pietyMonster;

	public PietyMonsterBehaviour PietyMonsterBehaviour;

	public int Spitsthrown;

	public GameObject SpitPrefab;

	public List<float> HorPositions = new List<float>();

	[Tooltip("Minimun spit distance expressed in Unity Units")]
	public float SpitDistance = 6f;

	public GameObject Target { get; set; }

	public int CurrentSpitAmount { get; set; }

	public float SpitDamage { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_pietyMonster = (PietyMonster)base.EntityOwner;
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		SetCurrentSpitAmount();
		if (PietyMonsterBehaviour.PlayerSeen && Target == null)
		{
			Target = _pietyMonster.PietyRootsManager.Target;
		}
		if (!Target)
		{
			return;
		}
		if (Vector2.Distance(_pietyMonster.GetPosition(), Target.transform.position) >= SpitDistance && PietyMonsterBehaviour.CanSpit)
		{
			if (!PietyMonsterBehaviour.Spiting)
			{
				PietyMonsterBehaviour.Spiting = true;
			}
		}
		else if (PietyMonsterBehaviour.Spiting)
		{
			PietyMonsterBehaviour.Spiting = false;
		}
	}

	public void Spit()
	{
		if (SpitPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(SpitPrefab, _pietyMonster.SpitingMouth.GetPosition(), Quaternion.identity);
			Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile.ThornProjectile component = gameObject.GetComponent<Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile.ThornProjectile>();
			component.GetComponentInChildren<IProjectileAttack>().SetProjectileWeaponDamage((int)SpitDamage);
			component.Target = Target.transform;
			component.SetOwner(_pietyMonster);
			float horSpitPosition = GetHorSpitPosition();
			Vector2 vector = new Vector2(horSpitPosition, _pietyMonster.PietyRootsManager.Collider.bounds.min.y);
			component.Throw(vector);
		}
	}

	private void SetCurrentSpitAmount()
	{
		float missingRatio = _pietyMonster.Stats.Life.MissingRatio;
		if (missingRatio > 0.75f)
		{
			CurrentSpitAmount = 1;
		}
		else if (missingRatio > 0.5f)
		{
			CurrentSpitAmount = 2;
		}
		else if (missingRatio >= 0f)
		{
			CurrentSpitAmount = 3;
		}
	}

	public void RefillSpitPositions()
	{
		HorPositions.Clear();
		Vector2 vector = new Vector2(Target.transform.position.x, Target.transform.position.y);
		for (int i = 0; i < CurrentSpitAmount; i++)
		{
			if (i == 0)
			{
				HorPositions.Add(vector.x);
			}
			else if (i % 2 != 0)
			{
				HorPositions.Add(vector.x + (float)i * Random.Range(1f, 2.5f));
			}
			else
			{
				HorPositions.Add(vector.x - (float)i * Random.Range(1f, 2.5f));
			}
		}
	}

	public float GetHorSpitPosition()
	{
		float result = Target.transform.position.x;
		if (HorPositions.Count > 0)
		{
			int index = Random.Range(0, HorPositions.Count);
			result = HorPositions[index];
			HorPositions.RemoveAt(index);
		}
		return result;
	}
}
