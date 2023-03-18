using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost;

public class BellGhostFloatingMotion : Trait
{
	public float amplitudeX = 10f;

	public float amplitudeY = 5f;

	private float index;

	public float speedX = 1f;

	public float speedY = 2f;

	public bool UseSlerp;

	public Vector2 Offset;

	public bool IsFloating { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsFloating && base.EntityOwner.SpriteRenderer.isVisible)
		{
			Floating();
		}
	}

	private void Floating()
	{
		index += Time.deltaTime;
		float x = amplitudeX * Mathf.Cos(speedX * index);
		float y = Mathf.Sin(speedY * index) * amplitudeY;
		Vector3 vector = new Vector3(x, y, 0f) + (Vector3)Offset;
		base.transform.localPosition = ((!UseSlerp) ? vector : Vector3.Slerp(base.transform.localPosition, vector, Time.deltaTime));
	}
}
