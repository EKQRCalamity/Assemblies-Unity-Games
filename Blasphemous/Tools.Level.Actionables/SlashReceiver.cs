using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Tools.Level.Actionables;

[RequireComponent(typeof(Collider2D))]
public class SlashReceiver : PersistentObject, IActionable, IDamageable
{
	public enum DAMAGEABLE_DIRECTION_LOCK
	{
		BOTH,
		RIGHT,
		LEFT
	}

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public GameObject[] OnHitUse;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private DAMAGEABLE_DIRECTION_LOCK receiveHitsFrom;

	[SerializeField]
	[EventRef]
	[BoxGroup("Audio", true, false, 0)]
	private string HitSoundFx;

	public bool Locked { get; set; }

	public void Use()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = 100f;
		Hit hit2 = hit;
		Damage(hit2);
	}

	public void Damage(Hit hit = default(Hit))
	{
		if (receiveHitsFrom != 0)
		{
			bool flag = Core.Logic.Penitent.transform.position.x > base.transform.position.x;
			if ((receiveHitsFrom == DAMAGEABLE_DIRECTION_LOCK.RIGHT && !flag) || (receiveHitsFrom == DAMAGEABLE_DIRECTION_LOCK.LEFT && flag))
			{
				return;
			}
		}
		HitReaction(hit);
		PlayHitSoundFx();
	}

	private void HitReaction(Hit hit)
	{
		for (int i = 0; i < OnHitUse.Length; i++)
		{
			IActionable[] components = OnHitUse[i].GetComponents<IActionable>();
			components.ForEach(delegate(IActionable actionable)
			{
				if (!(actionable is SlashReceiver))
				{
					if (hit.DamageType == DamageArea.DamageType.Heavy && actionable is ActionableForce)
					{
						(actionable as ActionableForce).HeavyUse();
					}
					else
					{
						actionable.Use();
					}
				}
			});
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}

	private void PlayHitSoundFx()
	{
		if (!string.IsNullOrEmpty(HitSoundFx))
		{
			Core.Audio.PlaySfx(HitSoundFx);
		}
	}
}
