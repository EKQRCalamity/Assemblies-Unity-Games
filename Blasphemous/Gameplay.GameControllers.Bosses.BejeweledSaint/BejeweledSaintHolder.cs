using DamageEffect;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BejeweledSaintHolder : MonoBehaviour, IDamageable
{
	public static Core.SimpleEvent OnHolderCollapse;

	public int Id;

	public AnimationCurve AppearingMoveCurve;

	private Transform _defaultParent;

	[EventRef]
	public string DamageSoundId;

	[EventRef]
	public string ShieldedSoundId;

	[EventRef]
	public string DownFallSoundId;

	public int MaxLife;

	private int _currentLife;

	public bool IsCollapsed;

	private Vector2 _defaultLocalPosition;

	public Animator Animator { get; set; }

	public SpriteRenderer SpriteRenderer { get; set; }

	public DamageEffectScript DamageEffect { get; set; }

	public BoxCollider2D DamageArea { get; private set; }

	private void Start()
	{
		Animator = GetComponent<Animator>();
		SpriteRenderer = GetComponent<SpriteRenderer>();
		DamageEffect = GetComponent<DamageEffectScript>();
		DamageArea = GetComponent<BoxCollider2D>();
		Animator.SetInteger("ID", Id);
		_currentLife = MaxLife;
		_defaultLocalPosition = new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
		_defaultParent = base.transform.parent;
	}

	public void Damage(Hit hit)
	{
		DamageEffect.Blink(0f, 0.1f);
		_currentLife--;
		Core.Audio.PlaySfx(DamageSoundId);
		if (_currentLife <= 0 && !IsCollapsed)
		{
			IsCollapsed = true;
			GetDown();
			Animator.SetTrigger("DISAPPEAR");
			Core.Audio.PlaySfx(DownFallSoundId);
		}
	}

	public void EnableDamageArea(bool enableDamageArea)
	{
		if (!(DamageArea == null))
		{
			DamageArea.enabled = enableDamageArea;
		}
	}

	public void Heal()
	{
		_currentLife = MaxLife;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void GetDown()
	{
		base.transform.parent = base.transform.root;
		if (OnHolderCollapse != null)
		{
			OnHolderCollapse();
		}
	}

	public void SetDefaultLocalPosition()
	{
		base.transform.parent = _defaultParent;
		IsCollapsed = false;
		base.transform.localPosition = _defaultLocalPosition;
		SpriteRenderer.enabled = true;
		DamageArea.GetComponent<BoxCollider2D>().enabled = true;
	}

	public void OnDisappear()
	{
		SpriteRenderer.enabled = false;
		DamageArea.GetComponent<BoxCollider2D>().enabled = false;
	}

	public bool BleedOnImpact()
	{
		return true;
	}

	public bool SparkOnImpact()
	{
		return true;
	}
}
