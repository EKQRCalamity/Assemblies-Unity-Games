using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Sparks;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SwordSpark : MonoBehaviour
{
	public enum SwordSparkType
	{
		swordSpark_1,
		swordSpark_2,
		swordSpark_3
	}

	public SwordSparkType sparkType;

	private SpriteRenderer swordSparkSpriteRenderer;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private void Awake()
	{
		swordSparkSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		_penitent = Core.Logic.Penitent;
	}

	private void Update()
	{
		setOrientation(_penitent.Status.Orientation);
	}

	protected void setOrientation(EntityOrientation orientation)
	{
		if (orientation == EntityOrientation.Left && !swordSparkSpriteRenderer.flipX)
		{
			swordSparkSpriteRenderer.flipX = true;
		}
		if (orientation == EntityOrientation.Right && swordSparkSpriteRenderer.flipX)
		{
			swordSparkSpriteRenderer.flipX = false;
		}
	}
}
