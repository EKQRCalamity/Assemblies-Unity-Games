using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

public class PenitentStillFrameEffect : SimpleVFX
{
	public float duration = 0.3f;

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		SetOrientationByPlayer();
		Sprite sprite = Core.Logic.Penitent.SpriteRenderer.sprite;
		if (_spriteRenderers.Length != 0)
		{
			_spriteRenderers[0].sprite = sprite;
			base.transform.localScale = Vector3.one;
			base.transform.localPosition += Vector3.down * 0.07f;
			base.transform.DOPunchScale(Vector3.one * 0.15f, duration, 1, 0f).SetEase(Ease.InOutBack);
			_spriteRenderers[0].color = new Color(_spriteRenderers[0].color.r, _spriteRenderers[0].color.g, _spriteRenderers[0].color.b, 0.3f);
			_spriteRenderers[0].DOFade(0f, duration);
		}
	}
}
