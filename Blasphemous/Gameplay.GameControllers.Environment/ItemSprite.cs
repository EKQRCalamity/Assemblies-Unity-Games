using UnityEngine;

namespace Gameplay.GameControllers.Environment;

public class ItemSprite : MonoBehaviour
{
	private Collider2D trigger;

	private SpriteRenderer spriteRenderer;

	private bool touched;

	private bool show;

	private Rigidbody2D rb;

	private void Start()
	{
		rb = GetComponentInParent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		touched = false;
		spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
		trigger = rb.GetComponent<Collider2D>();
		trigger.enabled = false;
	}

	private void OnCollisionStay2D(Collision2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer("Floor") || col.gameObject.layer == LayerMask.NameToLayer("OneWayDown"))
		{
			touched = true;
		}
	}

	private void Update()
	{
		if (show && spriteRenderer.color.a < 1f)
		{
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 2f);
		}
		else if (!trigger.enabled)
		{
			trigger.enabled = true;
		}
		if (touched && rb.velocity.x == 0f && rb.velocity.y == 0f)
		{
			show = true;
		}
	}

	public void Hide()
	{
		touched = false;
	}
}
