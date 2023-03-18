using Gameplay.GameControllers.Bosses.Snake;
using UnityEngine;

public class SnakeAnimationEventsController : MonoBehaviour
{
	[SerializeField]
	public SnakeBehaviour SnakeBehaviour;

	[SerializeField]
	private bool AlwaysListensEvents;

	private SpriteRenderer spriteRenderer;

	private bool listenEvents => AlwaysListensEvents || spriteRenderer.IsVisibleFrom(Camera.main);

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Animation_OnMeleeAttackStarts()
	{
		if (listenEvents)
		{
			SnakeBehaviour.OnMeleeAttackStarts();
		}
	}

	public void Animation_OnMeleeAttackFinished()
	{
		if (listenEvents)
		{
			SnakeBehaviour.OnMeleeAttackFinished();
		}
	}

	public void DoActivateCollisionsOpenMouth(bool act)
	{
		if (listenEvents)
		{
			SnakeBehaviour.DoActivateCollisionsOpenMouth(act);
		}
	}

	public void DoActivateCollisionsIdle(bool act)
	{
		if (listenEvents)
		{
			SnakeBehaviour.DoActivateCollisionsIdle(act);
		}
	}

	public void Animation_SetWeapon(SnakeBehaviour.SNAKE_WEAPONS weaponToUse)
	{
		if (listenEvents)
		{
			SnakeBehaviour.SetWeapon(weaponToUse);
		}
	}

	public void Animation_PlaySnakeGrunt1()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeGrunt1();
		}
	}

	public void Animation_PlaySnakeGrunt2()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeGrunt2();
		}
	}

	public void Animation_PlaySnakeGrunt3()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeGrunt3();
		}
	}

	public void Animation_PlaySnakeGrunt4()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeGrunt4();
		}
	}

	public void Animation_PlaySnakeBite()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeBite();
		}
	}

	public void Animation_PlaySnakeBack()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeBack();
		}
	}

	public void Animation_PlaySnakeTongueExplosion()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeTongueExplosion();
		}
	}

	public void Animation_PlayDeathStinger()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlaySnakeDeathStinger();
		}
	}

	public void Animation_PlayOneShot(string eventId)
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.PlayOneShot_AUDIO(eventId);
		}
	}

	public void Animation_PlayAudio(string eventId)
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.Play_AUDIO(eventId);
		}
	}

	public void Animation_StopAudio(string eventId)
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.Stop_AUDIO(eventId);
		}
	}

	public void Animation_StopAllAudios()
	{
		if (listenEvents)
		{
			SnakeBehaviour.Snake.Audio.StopAll();
		}
	}
}
