using System.Linq;
using UnityEngine;

public class MapPlayerAnimationController : AbstractMapPlayerComponent
{
	public enum Direction
	{
		Left,
		Right
	}

	public enum State
	{
		Idle,
		Walk
	}

	private const int DJIMMI_CODE_LENGTH = 16;

	private const float MAX_TIME_FOR_DJIMMI_CODE = 2f;

	private int[] djimmiCodeA = new int[16]
	{
		0, 90, -180, -90, 0, 90, -180, -90, 0, 90,
		-180, -90, 0, 90, -180, -90
	};

	private int[] djimmiCodeB = new int[16]
	{
		0, -90, -180, 90, 0, -90, -180, 90, 0, -90,
		-180, 90, 0, -90, -180, 90
	};

	public bool facingUpwards;

	[SerializeField]
	private SpriteRenderer Cuphead;

	[SerializeField]
	private SpriteRenderer Mugman;

	[SerializeField]
	private SpriteRenderer Chalice;

	[SerializeField]
	private SpriteRenderer[] ghostInPortal;

	[SerializeField]
	private SpriteRenderer portal;

	[SerializeField]
	private MapPlayerDust dustEffect;

	private MapSpritePlaySound current;

	private Trilean2 axis;

	private bool onBridge;

	private float directionRotation;

	private int[] djimmiCodeEntry = new int[16];

	private float[] djimmiCodeTimeStamp = new float[16];

	public Direction direction { get; private set; }

	public State state { get; private set; }

	public SpriteRenderer spriteRenderer { get; set; }

	public void Init(MapPlayerPose pose)
	{
		Cuphead.enabled = false;
		Mugman.enabled = false;
		ghostInPortal[0].sortingLayerName = "Effects";
		ghostInPortal[1].sortingLayerName = "Effects";
		portal.sortingLayerName = "Effects";
		PlayerId id = base.player.id;
		if (id == PlayerId.PlayerOne || id != PlayerId.PlayerTwo)
		{
			spriteRenderer = ((!PlayerManager.player1IsMugman) ? Cuphead : Mugman);
			base.animator.SetInteger("Player", 0);
		}
		else
		{
			base.animator.SetInteger("Player", 1);
			spriteRenderer = ((!PlayerManager.player1IsMugman) ? Mugman : Cuphead);
		}
		spriteRenderer.enabled = true;
		switch (pose)
		{
		case MapPlayerPose.Default:
			state = State.Idle;
			break;
		case MapPlayerPose.Joined:
		case MapPlayerPose.Won:
			base.animator.Play((!PlayerManager.playerWasChalice[(int)base.player.id]) ? "Jump" : "WinChalice_Loop");
			if (PlayerManager.playerWasChalice[(int)base.player.id])
			{
				if (PlayerManager.player1IsMugman)
				{
					ghostInPortal[(int)base.player.id].enabled = false;
				}
				else
				{
					ghostInPortal[(int)(1 - base.player.id)].enabled = false;
				}
				if (base.player.id == PlayerId.PlayerTwo)
				{
					base.transform.localScale = new Vector3(-1f, 1f);
				}
			}
			break;
		}
		SetProperties();
	}

	private void MovePortalSwapToFront()
	{
		Chalice.sortingLayerName = "Effects";
	}

	private void Update()
	{
		if (base.player.state == MapPlayerController.State.Stationary)
		{
			SetStationary();
			return;
		}
		if (!MapPlayerController.CanMove())
		{
			SetStationary();
			return;
		}
		state = ((new Vector2(base.player.input.actions.GetAxis(0), base.player.input.actions.GetAxis(1)).magnitude > 0.3f) ? State.Walk : State.Idle);
		SetProperties();
		UpdateDjimmiCodeTimer();
	}

	private void SetStationary()
	{
		state = State.Idle;
		axis.x = 0;
		axis.y = 0;
		SetProperties();
	}

	public void CompleteJump()
	{
		base.animator.SetTrigger("OnJumpComplete");
	}

	private void SetProperties()
	{
		if (state == State.Walk)
		{
			axis.x = base.player.input.GetAxisInt(PlayerInput.Axis.X);
			axis.y = base.player.input.GetAxisInt(PlayerInput.Axis.Y);
			if ((int)axis.x == -1)
			{
				spriteRenderer.transform.SetScale(-1f);
			}
			else
			{
				spriteRenderer.transform.SetScale(1f);
			}
		}
		base.animator.SetInteger("X", axis.x);
		base.animator.SetInteger("Y", axis.y);
		base.animator.SetInteger("Speed", (state != 0) ? 1 : 0);
		SetDirectionRotation();
	}

	private void SetDirectionRotation()
	{
		facingUpwards = (int)axis.y > 0;
		if ((int)axis.x == 1 && (int)axis.y == 1)
		{
			directionRotation = -45f;
		}
		else if ((int)axis.x == 1 && (int)axis.y == 0)
		{
			directionRotation = -90f;
		}
		else if ((int)axis.x == 1 && (int)axis.y == -1)
		{
			directionRotation = -135f;
		}
		else if ((int)axis.x == 0 && (int)axis.y == 1)
		{
			directionRotation = 0f;
		}
		else if ((int)axis.x == 0 && (int)axis.y == 0)
		{
			directionRotation = 0f;
		}
		else if ((int)axis.x == 0 && (int)axis.y == -1)
		{
			directionRotation = -180f;
		}
		else if ((int)axis.x == -1 && (int)axis.y == 1)
		{
			directionRotation = 45f;
		}
		else if ((int)axis.x == -1 && (int)axis.y == 0)
		{
			directionRotation = 90f;
		}
		else if ((int)axis.x == -1 && (int)axis.y == -1)
		{
			directionRotation = 135f;
		}
		UpdateDjimmiCode((int)directionRotation);
	}

	private void UpdateDjimmiCode(int direction)
	{
		if (direction == -45 || direction == -135 || direction == 45 || direction == 135 || direction == djimmiCodeEntry[djimmiCodeEntry.Length - 1])
		{
			return;
		}
		for (int i = 0; i < djimmiCodeEntry.Length - 1; i++)
		{
			djimmiCodeEntry[i] = djimmiCodeEntry[i + 1];
			djimmiCodeTimeStamp[i] = djimmiCodeTimeStamp[i + 1];
		}
		djimmiCodeEntry[djimmiCodeEntry.Length - 1] = direction;
		djimmiCodeTimeStamp[djimmiCodeEntry.Length - 1] = 2f;
		if (djimmiCodeTimeStamp[0] > 0f && (djimmiCodeEntry.SequenceEqual(djimmiCodeA) || djimmiCodeEntry.SequenceEqual(djimmiCodeB)))
		{
			for (int j = 0; j < djimmiCodeEntry.Length; j++)
			{
				djimmiCodeEntry[j] = 0;
				djimmiCodeTimeStamp[j] = 0f;
			}
			base.player.TryActivateDjimmi();
		}
	}

	private void UpdateDjimmiCodeTimer()
	{
		for (int i = 0; i < djimmiCodeTimeStamp.Length; i++)
		{
			djimmiCodeTimeStamp[i] -= CupheadTime.Delta;
		}
	}

	private void WalkStepLeft()
	{
		if (spriteRenderer == Cuphead)
		{
			if (current != null)
			{
				current.PlaySoundRight(isP1: true);
			}
			else
			{
				AudioManager.Play("player_map_walk_one_p1");
			}
		}
		else if (current != null)
		{
			current.PlaySoundRight(isP1: false);
		}
		else
		{
			AudioManager.Play("player_map_walk_one_p2");
		}
		dustEffect.Create(base.transform.position, directionRotation, isLeft: true, spriteRenderer.sortingOrder);
	}

	private void WalkStepRight()
	{
		if (spriteRenderer == Cuphead)
		{
			if (current != null)
			{
				current.PlaySoundRight(isP1: true);
			}
			else
			{
				AudioManager.Play("player_map_walk_one_p1");
			}
		}
		else if (current != null)
		{
			current.PlaySoundRight(isP1: false);
		}
		else
		{
			AudioManager.Play("player_map_walk_two_p2");
		}
		dustEffect.Create(base.transform.position, directionRotation, isLeft: false, spriteRenderer.sortingOrder);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if ((bool)collider.GetComponent<MapSpritePlaySound>())
		{
			current = collider.GetComponent<MapSpritePlaySound>();
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		if ((bool)collider.GetComponent<MapSpritePlaySound>())
		{
			current = null;
		}
	}

	private void AniEvent_YawnSFX()
	{
		if (base.player.id == PlayerId.PlayerOne)
		{
			AudioManager.Play("worldmap_playeryawn");
			emitAudioFromObject.Add("worldmap_playeryawn");
		}
	}

	private void AniEvent_GhostSwapSFX()
	{
		AudioManager.Play("sfx_DLC_WorldMap_GhostSwap");
		emitAudioFromObject.Add("sfx_DLC_WorldMap_GhostSwap");
	}
}
