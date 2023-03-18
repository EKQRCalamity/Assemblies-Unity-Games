using System;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.Level.Actionables;

[SelectionBase]
public class BreakableObject : PersistentObject, IActionable, IDamageable
{
	private class BreakablePersistenceData : PersistentManager.PersistentData
	{
		public bool broken;

		public BreakablePersistenceData(string id)
			: base(id)
		{
		}
	}

	public static Core.ObjectEvent OnDead;

	public Core.SimpleEvent OnBreak;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string destroySound;

	[SerializeField]
	[BoxGroup("Animation", true, false, 0)]
	private string breakTrigger = "BROKEN";

	[FoldoutGroup("Damage Settings", 0)]
	public bool bleedOnImpact;

	[FoldoutGroup("Damage Settings", 0)]
	public bool sparksOnImpact = true;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private DamageArea damageArea;

	private bool destroyed;

	public bool SoftDisableWhenBroken;

	private bool IsBroken => Core.Logic.BreakableManager.ContainsBreakable(GetHashId);

	private int GetHashId => Animator.StringToHash(base.gameObject.name + Core.LevelManager.currentLevel.LevelName);

	public bool Locked { get; set; }

	private void Start()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		LogicManager logic = Core.Logic;
		logic.OnUsePrieDieu = (Core.SimpleEvent)Delegate.Combine(logic.OnUsePrieDieu, new Core.SimpleEvent(OnUsePrieDieu));
	}

	private void OnUsePrieDieu()
	{
		Restore();
	}

	private void OnDestroy()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		LogicManager logic = Core.Logic;
		logic.OnUsePrieDieu = (Core.SimpleEvent)Delegate.Remove(logic.OnUsePrieDieu, new Core.SimpleEvent(OnUsePrieDieu));
	}

	private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
	{
	}

	public void Damage(Hit hit = default(Hit))
	{
		if (!(animator == null) && !destroyed)
		{
			if (hit.AttackingEntity != null)
			{
				Transform attacker = hit.AttackingEntity.transform;
				spriteRenderer.flipX = GetAttackDirection(attacker) > 0f;
			}
			Core.Audio.PlaySfx(destroySound);
			animator.SetTrigger(breakTrigger);
			damageArea.DamageAreaCollider.enabled = false;
			destroyed = true;
			if (OnBreak != null)
			{
				OnBreak();
			}
			Core.InventoryManager.OnBreakBreakable(this);
			Core.Logic.BreakableManager.AddBreakable(GetHashId);
		}
	}

	public void Restore()
	{
		if (destroyed)
		{
			destroyed = false;
			damageArea.DamageAreaCollider.enabled = true;
			animator.enabled = true;
			spriteRenderer.enabled = true;
			animator.Play("idle");
			Core.Logic.BreakableManager.RemoveBreakable(GetHashId);
		}
	}

	private void SoftDisable()
	{
		damageArea.DamageAreaCollider.enabled = false;
		animator.Play("broken");
	}

	private void HardDisable()
	{
		damageArea.DamageAreaCollider.enabled = false;
		animator.enabled = false;
		spriteRenderer.enabled = false;
	}

	public void Use()
	{
		Damage();
		if (OnDead != null)
		{
			OnDead(base.gameObject);
		}
	}

	private float GetAttackDirection(Transform attacker)
	{
		return Mathf.Sign(attacker.position.x - damageArea.transform.position.x);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public bool BleedOnImpact()
	{
		return bleedOnImpact;
	}

	public bool SparkOnImpact()
	{
		return sparksOnImpact;
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		BreakablePersistenceData breakablePersistenceData = CreatePersistentData<BreakablePersistenceData>();
		breakablePersistenceData.broken = destroyed;
		return breakablePersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		BreakablePersistenceData breakablePersistenceData = (BreakablePersistenceData)data;
		destroyed = breakablePersistenceData.broken;
		if (IsBroken)
		{
			if (SoftDisableWhenBroken)
			{
				SoftDisable();
			}
			else
			{
				HardDisable();
			}
		}
		else
		{
			Restore();
		}
	}
}
