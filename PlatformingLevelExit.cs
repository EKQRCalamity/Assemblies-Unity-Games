using System;
using System.Collections;
using UnityEngine;

public class PlatformingLevelExit : AbstractCollidableObject
{
	[SerializeField]
	[Range(200f, 1500f)]
	private float _exitDistance = 500f;

	[SerializeField]
	private float onCompleteWaitTime = 2f;

	private bool _activated;

	private bool _exited;

	public static event Action OnWinStartEvent;

	public static event Action OnWinCompleteEvent;

	private void FixedUpdate()
	{
		if (_activated)
		{
			if (_exited)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				AbstractPlayerController player = PlayerManager.GetPlayer((i != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
				if (!(player == null) && player.center.x > base.transform.position.x + _exitDistance)
				{
					_exited = true;
					if (PlatformingLevelExit.OnWinCompleteEvent != null)
					{
						PlatformingLevelExit.OnWinCompleteEvent();
						PlatformingLevelExit.OnWinCompleteEvent = null;
					}
					break;
				}
			}
			return;
		}
		for (int j = 0; j < 2; j++)
		{
			AbstractPlayerController player2 = PlayerManager.GetPlayer((j != 0) ? PlayerId.PlayerTwo : PlayerId.PlayerOne);
			if (!(player2 == null) && !player2.IsDead && player2.center.x > base.transform.position.x)
			{
				_activated = true;
				if (PlatformingLevelExit.OnWinStartEvent != null)
				{
					PlatformingLevelExit.OnWinStartEvent();
					PlatformingLevelExit.OnWinStartEvent = null;
				}
				PlatformingLevelEnd.Win();
				StartCoroutine(on_win_complete_cr());
				break;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PlatformingLevelExit.OnWinStartEvent = null;
		PlatformingLevelExit.OnWinCompleteEvent = null;
	}

	private IEnumerator on_win_complete_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, onCompleteWaitTime);
		if (PlatformingLevelExit.OnWinCompleteEvent != null)
		{
			PlatformingLevelExit.OnWinCompleteEvent();
			PlatformingLevelExit.OnWinCompleteEvent = null;
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		DrawGizmos(0.5f);
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		DrawGizmos(1f);
	}

	private void DrawGizmos(float a)
	{
		Gizmos.color = new Color(0f, 1f, 0f, a);
		Vector3 vector = base.baseTransform.position + new Vector3(_exitDistance, 0f, 0f);
		Gizmos.DrawLine(base.baseTransform.position, vector);
		Gizmos.DrawLine(vector + new Vector3(0f, -5000f, 0f), vector + new Vector3(0f, 5000f, 0f));
		Gizmos.color = Color.white;
	}
}
