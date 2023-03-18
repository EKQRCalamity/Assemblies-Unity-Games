using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Healing;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class HealingAura : MonoBehaviour
{
	private Animator _auraAnimator;

	private SpriteRenderer _auraRenderer;

	private void Awake()
	{
		_auraAnimator = GetComponent<Animator>();
		_auraRenderer = GetComponent<SpriteRenderer>();
	}

	public void StartAura(EntityOrientation orientation)
	{
		if (!(_auraAnimator == null))
		{
			SetOrientatation(orientation);
			_auraAnimator.SetBool("HEALING", value: true);
			_auraAnimator.Play(0, 0, 0f);
		}
	}

	public void StopAura()
	{
		if (!(_auraAnimator == null))
		{
			_auraAnimator.SetBool("HEALING", value: false);
		}
	}

	private void SetOrientatation(EntityOrientation orientation)
	{
		_auraRenderer.flipX = orientation == EntityOrientation.Left;
	}

	public void PlayHealingExplosion()
	{
		Core.Logic.Penitent.Audio.HealingExplosion();
	}
}
