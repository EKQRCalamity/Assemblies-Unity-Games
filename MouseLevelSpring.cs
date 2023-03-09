using UnityEngine;

public class MouseLevelSpring : ParrySwitch
{
	[SerializeField]
	private Effect smallExplosion;

	public float knockUpHeight = -1.5f;

	private bool isLaunched;

	private Vector2 velocity;

	private float gravity;

	private float offset = 120f;

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (phase == CollisionPhase.Enter && hit.GetComponent<MouseLevelCanMouse>() != null && !isLaunched)
		{
			smallExplosion.Create(base.transform.position);
			base.animator.SetTrigger("OnDeath");
		}
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		player.GetComponent<LevelPlayerMotor>().OnTrampolineKnockUp(knockUpHeight);
		if (!isLaunched)
		{
			base.animator.SetTrigger("OnLaunch");
		}
	}

	private void GotRunOver()
	{
		GetComponent<Collider2D>().enabled = false;
		base.gameObject.SetActive(value: false);
	}

	public void LaunchSpring(Vector2 position, Vector2 velocity, float gravity)
	{
		if (base.gameObject.activeSelf)
		{
			base.animator.Play("Flip");
		}
		base.transform.position = position;
		this.velocity = velocity;
		this.gravity = gravity;
		isLaunched = true;
		base.gameObject.SetActive(value: true);
		GetComponent<Collider2D>().enabled = true;
	}

	private void Update()
	{
		if (!isLaunched)
		{
			return;
		}
		base.transform.AddPosition(velocity.x * (float)CupheadTime.Delta, velocity.y * (float)CupheadTime.Delta);
		velocity.y -= gravity * (float)CupheadTime.Delta;
		if (base.transform.position.y < (float)Level.Current.Ground + offset)
		{
			base.transform.SetPosition(null, (float)Level.Current.Ground + offset);
			if (isLaunched)
			{
				Landed();
			}
			isLaunched = false;
		}
	}

	private void Landed()
	{
		base.animator.SetTrigger("OnLand");
		AudioManager.Play("level_mouse_can_springboard_land");
		emitAudioFromObject.Add("level_mouse_can_springboard_land");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		smallExplosion = null;
	}
}
