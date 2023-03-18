using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.Animator;

[RequireComponent(typeof(SpriteRenderer))]
public class DrownedCorpseHelmet : MonoBehaviour
{
	private Entity _owner;

	private SpriteRenderer _spriteRenderer;

	public void Initialize(Entity owner)
	{
		_owner = owner;
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnEnable()
	{
		if ((bool)_owner)
		{
			EntityOrientation orientation = _owner.Status.Orientation;
			SetOrientation(orientation);
		}
	}

	private void SetOrientation(EntityOrientation orientation)
	{
		_spriteRenderer.flipX = orientation == EntityOrientation.Left;
	}
}
