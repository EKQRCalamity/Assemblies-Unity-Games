using System.Collections;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Environment.Traps.FireTrap;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Tools.Items;

public class PR203ElmFireLoopEffect : ObjectEffect
{
	public int NumPulses = 3;

	public float WaitTimeToShowEachTrap = 0.1f;

	public float LightningChargeLapse = 0.05f;

	public float WaitTimeBetweenPulses = 0.1f;

	public float WaitTimeToHideEachTrap = 1f;

	public int BaseLightningDamage = 40;

	public GameObject ElmFireLoop;

	private readonly int numTraps = 7;

	private readonly float effectTimeToPulsesRatio = 0.6f;

	protected override void OnAwake()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(ElmFireLoop, penitent.transform.position + Vector3.up * 1000f, Quaternion.identity, createPoolIfNeeded: true);
		ElmFireTrapManager componentInChildren = objectInstance.GameObject.GetComponentInChildren<ElmFireTrapManager>();
		componentInChildren.InstantHideElmFireTraps();
	}

	protected override bool OnApplyEffect()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition();
		position += ((Core.Logic.Penitent.Status.Orientation != EntityOrientation.Left) ? (Vector3.right * 1.25f) : (Vector3.left * 1.25f));
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(ElmFireLoop, position, Quaternion.identity, createPoolIfNeeded: true);
		int num = ((Core.Logic.Penitent.Status.Orientation != EntityOrientation.Left) ? 1 : (-1));
		objectInstance.GameObject.transform.localScale = new Vector3(num, 1f, 1f);
		ElmFireTrapManager componentInChildren = objectInstance.GameObject.GetComponentInChildren<ElmFireTrapManager>();
		float final = Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final;
		componentInChildren.RecursiveSetUpTrapDamage((float)BaseLightningDamage * final);
		StartCoroutine(ElmFireLoopCoroutine(componentInChildren));
		return true;
	}

	private IEnumerator ElmFireLoopCoroutine(ElmFireTrapManager m)
	{
		m.InstantHideElmFireTraps();
		m.ShowElmFireTrapRecursively(m.elmFireTrapNodes[0], WaitTimeToShowEachTrap, LightningChargeLapse, applyChargingTimeToAll: true);
		yield return new WaitForSeconds(WaitTimeToShowEachTrap * (float)numTraps);
		m.EnableTraps();
		int realNumPulses = NumPulses;
		float addition = Core.Logic.Penitent.Stats.PrayerDurationAddition.Final;
		if (addition > 0f)
		{
			realNumPulses += (int)(effectTimeToPulsesRatio * addition);
		}
		for (int i = 0; i < realNumPulses; i++)
		{
			m.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			yield return new WaitForSeconds(LightningChargeLapse * (float)numTraps);
			if (i < realNumPulses - 1)
			{
				yield return new WaitForSeconds(WaitTimeBetweenPulses);
			}
		}
		m.DisableTraps();
		m.HideElmFireTrapRecursively(m.elmFireTrapNodes[0], WaitTimeToHideEachTrap);
		yield return new WaitForSeconds(WaitTimeToHideEachTrap * (float)numTraps);
	}
}
