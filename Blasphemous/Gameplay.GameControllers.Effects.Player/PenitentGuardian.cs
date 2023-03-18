using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player;

public class PenitentGuardian : MonoBehaviour
{
	protected SpriteRenderer GuardianSpriteRenderer;

	protected Gameplay.GameControllers.Penitent.Penitent Owner;

	public bool IsTriggered { get; set; }

	private void OnEnable()
	{
		if (GuardianSpriteRenderer == null)
		{
			GuardianSpriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (Owner == null)
		{
			Owner = Core.Logic.Penitent;
		}
		FadeIn();
	}

	public void SetOrientation(Hit hit)
	{
		if (!(Owner == null))
		{
			Vector3 position = hit.AttackingEntity.transform.position;
			GuardianSpriteRenderer.flipX = position.x <= Owner.transform.position.x;
		}
	}

	public void FadeIn()
	{
		if (!(GuardianSpriteRenderer == null))
		{
			GuardianSpriteRenderer.DOFade(1f, 0.1f);
		}
	}

	public void FadeOut()
	{
		if (!(GuardianSpriteRenderer == null))
		{
			GuardianSpriteRenderer.DOFade(0f, 0.1f);
		}
	}
}
