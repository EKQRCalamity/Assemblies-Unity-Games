using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Bosses.Amanecidas;
using UnityEngine;

public class LaudesArena : MonoBehaviour
{
	[Serializable]
	public struct ArenaByWeapon
	{
		public AmanecidaArena arena;

		public AmanecidasAnimatorInyector.AMANECIDA_WEAPON weapon;
	}

	public List<ArenaByWeapon> arenasByWeapon;

	private AmanecidaArena prevArena;

	private Amanecidas amanecida;

	public void SetNextArena(Amanecidas amanecida)
	{
		this.amanecida = amanecida;
		SetLaudesArena(amanecida, prevArena.transform.position);
	}

	public void SetLaudesArena(Amanecidas amanecida, Vector2 origin, bool onlySetBattleBounds = false)
	{
		this.amanecida = amanecida;
		AmanecidasAnimatorInyector.AMANECIDA_WEAPON wpn = amanecida.Behaviour.currentWeapon;
		AmanecidaArena arena = arenasByWeapon.Find((ArenaByWeapon x) => x.weapon == wpn).arena;
		if (prevArena != null && prevArena != arena)
		{
			prevArena.StartDeactivateArena();
		}
		arena.ActivateArena(amanecida, origin, onlySetBattleBounds);
		prevArena = arena;
	}

	public void StartIntro(AmanecidasAnimatorInyector.AMANECIDA_WEAPON wpn)
	{
		AmanecidaArena arena = arenasByWeapon.Find((ArenaByWeapon x) => x.weapon == wpn).arena;
		if (arena != null)
		{
			arena.StartIntro();
		}
		else
		{
			Debug.LogError("No arena found in LaudesArena for weapon: " + wpn);
		}
	}

	public void ActivateGameObjectsByWeaponFightPhase(AmanecidasAnimatorInyector.AMANECIDA_WEAPON wpn, AmanecidaArena.WEAPON_FIGHT_PHASE phase)
	{
		AmanecidaArena arena = arenasByWeapon.Find((ArenaByWeapon x) => x.weapon == wpn).arena;
		if (arena != null)
		{
			arena.ActivateArena(amanecida, arena.transform.position, onlySetBattleBounds: false, phase);
		}
		else
		{
			Debug.LogError("No arena found in LaudesArena for weapon: " + wpn);
		}
	}
}
