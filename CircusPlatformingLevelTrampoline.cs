using System.Collections;
using UnityEngine;

public class CircusPlatformingLevelTrampoline : AbstractCollidableObject
{
	public enum Direction
	{
		Left,
		Right
	}

	private const string BounceTrigger = "Bounce";

	private const string Sleep = "Sleep";

	[SerializeField]
	private float bounds;

	[SerializeField]
	private float AwakeningZone = 500f;

	public float maxSpeed;

	public float acceleration;

	public float knockUpHeight = -1.95f;

	private float velocityX;

	private Vector2 startPos;

	private Vector2 position;

	public Direction MoveDirection { get; set; }

	public AbstractPlayerController TrackingPlayer { get; set; }

	private void Start()
	{
		startPos = base.transform.position;
		StartCoroutine(loop_cr());
		StartCoroutine(sleep_sfx_cr());
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (phase == CollisionPhase.Enter || phase == CollisionPhase.Stay)
		{
			LevelPlayerMotor component = hit.GetComponent<LevelPlayerMotor>();
			if (component != null && component.Grounded)
			{
				base.animator.SetTrigger("Bounce");
				component.OnTrampolineKnockUp(knockUpHeight);
			}
		}
	}

	private IEnumerator loop_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		TrackingPlayer = PlayerManager.GetNext();
		while (TrackingPlayer == null)
		{
			yield return null;
		}
		float minBounds = base.transform.localPosition.x - bounds;
		float maxBounds = base.transform.localPosition.x + bounds;
		while (true)
		{
			if (!base.enabled)
			{
				yield return null;
				continue;
			}
			if (TrackingPlayer == null)
			{
				TrackingPlayer = PlayerManager.GetNext();
			}
			if (TrackingPlayer.transform.position.x > base.transform.position.x)
			{
				MoveDirection = Direction.Right;
			}
			else
			{
				MoveDirection = Direction.Left;
			}
			if (MoveDirection == Direction.Right)
			{
				if (base.transform.position.x < startPos.x + bounds)
				{
					velocityX += acceleration * (float)CupheadTime.Delta;
				}
				else
				{
					velocityX = 0f;
				}
			}
			else if (base.transform.position.x > startPos.x - bounds)
			{
				velocityX -= acceleration * (float)CupheadTime.Delta;
			}
			else
			{
				velocityX = 0f;
			}
			velocityX = Mathf.Clamp(velocityX, 0f - maxSpeed, maxSpeed);
			position = base.transform.localPosition;
			position.x += velocityX * (float)CupheadTime.Delta;
			if (position.x < minBounds)
			{
				position.x = minBounds;
				velocityX = 0f;
			}
			else if (position.x > maxBounds)
			{
				position.x = maxBounds;
				velocityX = 0f;
			}
			base.transform.localPosition = position;
			if (TrackingPlayer.IsDead)
			{
				TrackingPlayer = PlayerManager.GetNext();
			}
			CheckIfShouldSleep();
			yield return null;
		}
	}

	private void CheckIfShouldSleep()
	{
		Transform other = PlayerManager.GetPlayer(PlayerId.PlayerOne).transform;
		if (IsInBounds(other))
		{
			base.animator.SetBool("Sleep", value: false);
			if (PlayerManager.Multiplayer && IsInBounds(PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform))
			{
				TrackingPlayer = PlayerManager.GetNext();
			}
			else
			{
				TrackingPlayer = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			}
		}
		else if (PlayerManager.Multiplayer && IsInBounds(PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform))
		{
			base.animator.SetBool("Sleep", value: false);
			TrackingPlayer = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		}
		else
		{
			if (base.transform.position.x >= startPos.x + bounds || base.transform.position.x <= startPos.x - bounds)
			{
				base.animator.SetBool("Sleep", value: true);
			}
			TrackingPlayer = PlayerManager.GetNext();
		}
	}

	private IEnumerator sleep_sfx_cr()
	{
		while (true)
		{
			if (base.animator.GetBool("Sleep"))
			{
				yield return CupheadTime.WaitForSeconds(this, Random.Range(2f, 5f));
				if (base.animator.GetBool("Sleep"))
				{
					AudioManager.Play("circus_trampoline_sleep_boil");
					emitAudioFromObject.Add("circus_trampoline_sleep_boil");
				}
			}
			yield return null;
		}
	}

	private bool IsInBounds(Transform other)
	{
		float num = startPos.x - bounds;
		float num2 = startPos.x + bounds;
		return other.position.x < num2 + AwakeningZone && other.position.x > num - AwakeningZone;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(new Vector2(startPos.x + bounds, startPos.y), new Vector2(startPos.x - bounds, startPos.y));
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.DrawLine(new Vector2(base.transform.position.x + bounds, base.transform.position.y), new Vector2(base.transform.position.x - bounds, base.transform.position.y));
	}

	private void TrampolineBounceSFX()
	{
		AudioManager.Play("circus_trampoline_bounce");
		emitAudioFromObject.Add("circus_trampoline_bounce");
	}

	private void TrampolineIntroSFX()
	{
		AudioManager.Stop("circus_trampoline_idle_loop");
		AudioManager.Play("circus_trampoline_sleep_intro");
		emitAudioFromObject.Add("circus_trampoline_sleep_intro");
	}

	private void TrampolineOutroSFX()
	{
		AudioManager.Play("circus_trampoline_sleep_outro");
		emitAudioFromObject.Add("circus_trampoline_sleep_outro");
	}

	private void TrampolineStartIdleSFX()
	{
		AudioManager.PlayLoop("circus_trampoline_idle_loop");
		emitAudioFromObject.Add("circus_trampoline_idle_loop");
	}
}
