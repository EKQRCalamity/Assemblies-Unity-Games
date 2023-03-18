using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

[RequireComponent(typeof(BoxCollider2D))]
public class DivineBeamOrigin : MonoBehaviour
{
	public Vector2 OriginPosition { get; private set; }

	public BoxCollider2D Collider2D { get; private set; }

	private void Start()
	{
		Collider2D = GetComponent<BoxCollider2D>();
		OriginPosition = new Vector2(Collider2D.bounds.center.x, Collider2D.bounds.min.y);
	}
}
