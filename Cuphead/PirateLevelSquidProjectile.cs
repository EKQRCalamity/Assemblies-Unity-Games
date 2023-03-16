using UnityEngine;

public class PirateLevelSquidProjectile : AbstractMonoBehaviour
{
	public enum State
	{
		Init,
		Moving,
		Dead
	}

	public const float MAX_LIFETIME = 5f;

	private State state;

	private Vector2 velocity;

	private float gravity;

	private float lifetime;

	private void Update()
	{
		if (state == State.Moving)
		{
			if (lifetime > 5f)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			base.transform.AddPosition(velocity.x * (float)CupheadTime.Delta, velocity.y * (float)CupheadTime.Delta);
			velocity.y -= gravity * (float)CupheadTime.Delta;
		}
		lifetime += CupheadTime.Delta;
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.name == PlayerId.PlayerOne.ToString() || collider.name == PlayerId.PlayerTwo.ToString())
		{
			PirateLevelSquidInkOverlay.Current.Hit();
			CupheadLevelCamera.Current.Shake(4f, 0.3f);
			Die();
		}
		else if (collider.name == "Level_Ground")
		{
			Die();
		}
	}

	public void Create(Vector2 pos, Vector2 velocity, float gravity)
	{
		InstantiatePrefab<PirateLevelSquidProjectile>().Init(pos, velocity, gravity);
	}

	private void Init(Vector2 pos, Vector2 velocity, float gravity)
	{
		base.transform.position = pos;
		this.velocity = velocity;
		this.gravity = gravity;
		state = State.Moving;
	}

	private void Die()
	{
		base.animator.SetTrigger("OnDeath");
		GetComponent<Collider2D>().enabled = false;
		state = State.Dead;
	}

	private void OnDeathAnimationComplete()
	{
		Object.Destroy(base.gameObject);
	}
}
