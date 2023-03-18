using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Actionables;

[RequireComponent(typeof(Collider2D))]
public class PersistentBreakableObject : PersistentObject, IActionable, IDamageable
{
	public enum DAMAGEABLE_DIRECTION_LOCK
	{
		BOTH,
		RIGHT,
		LEFT
	}

	[Serializable]
	public struct DamageEvent
	{
		public string EventName;

		public int TriggerValue;
	}

	private class BreakablePersistentData : PersistentManager.PersistentData
	{
		public float Life;

		public BreakablePersistentData(string id)
			: base(id)
		{
		}
	}

	[BoxGroup("Design Settings", true, false, 0)]
	public bool DamageByHits;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("DamageByHits", true)]
	protected List<DamageEvent> DamageEvents;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public GameObject[] OnDestroy;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float health;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float RemainTimeAfterHit = 2f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private string brokenStateName = string.Empty;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private DAMAGEABLE_DIRECTION_LOCK breakableFrom;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool changeLayerOnDestroy = true;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	private string hitFxSound;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	private string breakFxSound;

	private ColorFlash colorFlash;

	private bool destroyed;

	private LayoutElement layoutElement;

	private bool layoutMode;

	private Collider2D wallCollider;

	public Animator animator;

	public bool Locked { get; set; }

	public bool Destroyed => destroyed;

	public void Use()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = 100f;
		Damage(hit);
	}

	public void Damage(Hit hit = default(Hit))
	{
		if (breakableFrom != 0)
		{
			bool flag = ((Core.Logic.Penitent.transform.position.x > base.transform.position.x) ? true : false);
			if ((breakableFrom == DAMAGEABLE_DIRECTION_LOCK.RIGHT && !flag) || (breakableFrom == DAMAGEABLE_DIRECTION_LOCK.LEFT && flag))
			{
				return;
			}
		}
		if (destroyed)
		{
			return;
		}
		HitReaction();
		float num = ((!DamageByHits) ? hit.DamageAmount : 1f);
		health -= num;
		SendHealthToAnimator();
		CheckDamageEvents((int)health);
		if (health <= 0f)
		{
			destroyed = true;
			if (changeLayerOnDestroy)
			{
				base.gameObject.layer = LayerMask.NameToLayer("Floor");
			}
			PlayFxSound(breakFxSound);
			StartCoroutine(DestroyWall());
		}
		else
		{
			PlayFxSound(hitFxSound);
		}
	}

	private void SendHealthToAnimator()
	{
		if (animator != null)
		{
			animator.SetFloat("HEALTH", health);
		}
	}

	private void HitReaction()
	{
		Core.Logic.ScreenFreeze.Freeze(0.01f, 0.05f);
		colorFlash.TriggerColorFlash();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void Awake()
	{
		layoutElement = GetComponent<LayoutElement>();
		wallCollider = GetComponent<Collider2D>();
		colorFlash = GetComponent<ColorFlash>();
		layoutMode = layoutElement.category == Category.Layout;
		SendHealthToAnimator();
	}

	private IEnumerator DestroyWall()
	{
		if (layoutMode)
		{
			SetColor(Color.green);
		}
		Core.Audio.PlaySfxOnCatalog("BREAK_WALL");
		for (int i = 0; i < OnDestroy.Length; i++)
		{
			if (!(OnDestroy[i] == null))
			{
				IActionable[] components = OnDestroy[i].GetComponents<IActionable>();
				components.ForEach(delegate(IActionable actionable)
				{
					actionable.Use();
				});
			}
		}
		yield return new WaitForSeconds(RemainTimeAfterHit);
		wallCollider.enabled = false;
		if (layoutMode)
		{
			Color color = layoutElement.SpriteRenderer.material.color;
			color.a = 0f;
			layoutElement.SpriteRenderer.material.color = color;
		}
	}

	private void SetColor(Color c)
	{
		Color color = new Color(c.r, c.g, c.b);
		layoutElement.SpriteRenderer.material.color = color;
	}

	private void PlayFxSound(string eventId)
	{
		if (!string.IsNullOrEmpty(eventId))
		{
			Core.Audio.PlaySfx(eventId);
		}
	}

	private void CheckDamageEvents(int currentLife)
	{
		if (!DamageByHits)
		{
			return;
		}
		foreach (DamageEvent item in DamageEvents.Where((DamageEvent damageEvent) => currentLife == damageEvent.TriggerValue))
		{
			if (!item.EventName.IsNullOrWhitespace())
			{
				string fsmEventName = item.EventName.Trim();
				PlayMakerFSM.BroadcastEvent(fsmEventName);
			}
		}
	}

	public virtual void SetDestroyedState()
	{
		if (brokenStateName != string.Empty)
		{
			animator.Play(brokenStateName);
		}
		if (changeLayerOnDestroy)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Floor");
		}
		if (layoutMode)
		{
			Color color = layoutElement.SpriteRenderer.material.color;
			color.a = 0f;
			layoutElement.SpriteRenderer.material.color = color;
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		BreakablePersistentData breakablePersistentData = CreatePersistentData<BreakablePersistentData>();
		breakablePersistentData.Life = health;
		return breakablePersistentData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		BreakablePersistentData breakablePersistentData = (BreakablePersistentData)data;
		health = breakablePersistentData.Life;
		SendHealthToAnimator();
		destroyed = health <= 0f;
		if (destroyed)
		{
			SetDestroyedState();
		}
		wallCollider.enabled = !destroyed;
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}
}
