using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Traits;

public class CustomShadowTrait : Trait
{
	public bool showShadow = true;

	public LayerMask groundLayerMask;

	public Vector2 castPoint;

	public Vector2 shadowOffset;

	public float raycastDistance;

	private SpriteRenderer _spriteRenderer;

	private RaycastHit2D[] results;

	protected override void OnStart()
	{
		base.OnStart();
		GenerateShadow();
	}

	private void GenerateShadow()
	{
		GameObject gameObject = new GameObject("Shadow");
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		results = new RaycastHit2D[1];
	}

	protected override void OnLateUpdate()
	{
		base.OnLateUpdate();
		if (showShadow)
		{
			_spriteRenderer.enabled = true;
			Vector2 vector = base.transform.position + (Vector3)castPoint;
			int num = Physics2D.LinecastNonAlloc(vector, vector + Vector2.down * raycastDistance, results, groundLayerMask);
			if (num > 0)
			{
				Debug.DrawLine(vector, vector + Vector2.down * raycastDistance, Color.green);
				Vector2 point = results[0].point;
				_spriteRenderer.transform.position = point + shadowOffset;
				_spriteRenderer.transform.up = results[0].normal;
			}
			else
			{
				_spriteRenderer.enabled = false;
				Debug.DrawLine(vector, vector + Vector2.down * raycastDistance, Color.red);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.black;
		Vector2 vector = base.transform.position + (Vector3)castPoint;
		Vector2 vector2 = vector + Vector2.down * raycastDistance;
		Gizmos.DrawWireSphere(vector, 0.2f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawWireCube(vector2, Vector2.one);
	}
}
