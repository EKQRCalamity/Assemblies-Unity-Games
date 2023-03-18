using System.Collections;
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
public class BreakableWall : PersistentObject, IActionable, IDamageable
{
	public enum DAMAGEABLE_DIRECTION_LOCK
	{
		BOTH,
		RIGHT,
		LEFT
	}

	private class WallPersistenceData : PersistentManager.PersistentData
	{
		public float Life;

		public WallPersistenceData(string id)
			: base(id)
		{
		}
	}

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
	private DAMAGEABLE_DIRECTION_LOCK breakableFrom;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private SecretReveal Secret = new SecretReveal();

	[BoxGroup("Audio", true, false, 0)]
	public bool AllowPlaySoundFx = true;

	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	public string BreakWallFx;

	private ColorFlash colorFlash;

	private bool destroyed;

	private LayoutElement layoutElement;

	private bool layoutMode;

	private Collider2D wallCollider;

	public bool Locked { get; set; }

	public bool Destroyed => destroyed;

	public void Use()
	{
		Hit hit = default(Hit);
		hit.DamageAmount = 100f;
		Hit hit2 = hit;
		Damage(hit2);
	}

	public void Damage(Hit hit = default(Hit))
	{
		if (!destroyed)
		{
			if (layoutMode && layoutElement != null)
			{
				colorFlash.TriggerColorFlash();
			}
			health -= hit.DamageAmount;
			if (health <= 0f)
			{
				destroyed = true;
				base.gameObject.layer = LayerMask.NameToLayer("Floor");
				StartCoroutine(DestroyWall());
			}
		}
	}

	private bool PenitentIsFacingDamageableDirection()
	{
		if (breakableFrom == DAMAGEABLE_DIRECTION_LOCK.BOTH)
		{
			return true;
		}
		if (!Core.ready || Core.Logic == null || Core.Logic.Penitent == null)
		{
			return false;
		}
		bool flag = Core.Logic.Penitent.transform.position.x > base.transform.position.x;
		return (breakableFrom == DAMAGEABLE_DIRECTION_LOCK.RIGHT && flag) || (breakableFrom == DAMAGEABLE_DIRECTION_LOCK.LEFT && !flag);
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
		if (layoutElement != null)
		{
			layoutMode = layoutElement.category == Category.Layout;
		}
	}

	private void Update()
	{
		if (!destroyed)
		{
			wallCollider.enabled = PenitentIsFacingDamageableDirection();
		}
	}

	private IEnumerator DestroyWall()
	{
		Secret.Reveal();
		if (layoutMode && layoutElement != null)
		{
			SetColor(Color.green);
		}
		PlayBreakSound();
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
		if (layoutElement != null && layoutMode)
		{
			Color color = layoutElement.SpriteRenderer.material.color;
			color.a = 0f;
			layoutElement.SpriteRenderer.material.color = color;
		}
	}

	private void PlayBreakSound()
	{
		if (AllowPlaySoundFx && !string.IsNullOrEmpty(BreakWallFx))
		{
			Core.Audio.PlaySfx(BreakWallFx);
		}
	}

	private void SetColor(Color c)
	{
		Color color = new Color(c.r, c.g, c.b);
		layoutElement.SpriteRenderer.material.color = color;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		WallPersistenceData wallPersistenceData = CreatePersistentData<WallPersistenceData>();
		wallPersistenceData.Life = health;
		return wallPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		WallPersistenceData wallPersistenceData = (WallPersistenceData)data;
		health = wallPersistenceData.Life;
		destroyed = health <= 0f;
		if (destroyed)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Floor");
			if (layoutMode && layoutElement != null)
			{
				Color color = layoutElement.SpriteRenderer.material.color;
				color.a = 0f;
				layoutElement.SpriteRenderer.material.color = color;
			}
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
