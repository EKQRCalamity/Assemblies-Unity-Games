using System.Collections;
using UnityEngine;

public class MountainPlatformingLevelScale : AbstractPausableComponent
{
	public enum ScaleState
	{
		rightDown,
		leftDown,
		still
	}

	[SerializeField]
	private float scaleSpeed;

	[SerializeField]
	private float scaleChangeAmount;

	[SerializeField]
	private MountainPlatformingLevelScalePart ScaleLeft;

	[SerializeField]
	private MountainPlatformingLevelScalePart ScaleRight;

	private Vector2 scaleLeftStart;

	private Vector2 scaleRightStart;

	public ScaleState scaleState;

	private void Start()
	{
		scaleLeftStart = ScaleLeft.transform.position;
		scaleRightStart = ScaleRight.transform.position;
		StartCoroutine(check_scale_cr());
	}

	private IEnumerator check_scale_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (ScaleRight.steppedOn)
			{
				if (!ScaleLeft.steppedOn)
				{
					if (ScaleRight.transform.position.y > scaleRightStart.y - scaleChangeAmount)
					{
						ScaleRight.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
						ChangeState(ScaleState.rightDown);
					}
					if (ScaleLeft.transform.position.y < scaleLeftStart.y + scaleChangeAmount)
					{
						ScaleLeft.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
					}
				}
				else if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
				{
					if (ScaleRight.transform.position.y < scaleRightStart.y)
					{
						ScaleRight.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
						ChangeState(ScaleState.leftDown);
					}
					else if (ScaleLeft.steppedOn)
					{
						ChangeState(ScaleState.still);
					}
					if (ScaleLeft.transform.position.y > scaleLeftStart.y)
					{
						ScaleLeft.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
					}
				}
			}
			else
			{
				if (ScaleRight.transform.position.y < scaleRightStart.y)
				{
					ScaleRight.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
					ChangeState(ScaleState.leftDown);
				}
				else if (!ScaleLeft.steppedOn)
				{
					ChangeState(ScaleState.still);
				}
				if (ScaleLeft.transform.position.y > scaleLeftStart.y)
				{
					ScaleLeft.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
				}
			}
			if (ScaleLeft.steppedOn)
			{
				if (!ScaleRight.steppedOn)
				{
					if (ScaleLeft.transform.position.y > scaleLeftStart.y - scaleChangeAmount)
					{
						ScaleLeft.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
						ChangeState(ScaleState.leftDown);
					}
					if (ScaleRight.transform.position.y < scaleRightStart.y + scaleChangeAmount)
					{
						ScaleRight.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
					}
				}
				else if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
				{
					if (ScaleLeft.transform.position.y < scaleLeftStart.y)
					{
						ScaleLeft.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
						ChangeState(ScaleState.rightDown);
					}
					else if (ScaleRight.steppedOn)
					{
						ChangeState(ScaleState.still);
					}
					if (ScaleRight.transform.position.y > scaleRightStart.y)
					{
						ScaleRight.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
					}
				}
			}
			else
			{
				if (ScaleLeft.transform.position.y < scaleLeftStart.y)
				{
					ScaleLeft.transform.AddPosition(0f, scaleSpeed * CupheadTime.FixedDelta);
					ChangeState(ScaleState.rightDown);
				}
				else if (!ScaleRight.steppedOn)
				{
					ChangeState(ScaleState.still);
				}
				if (ScaleRight.transform.position.y > scaleRightStart.y)
				{
					ScaleRight.transform.AddPosition(0f, (0f - scaleSpeed) * CupheadTime.FixedDelta);
				}
			}
			yield return wait;
		}
	}

	private void ChangeState(ScaleState state)
	{
		if (scaleState == state)
		{
			return;
		}
		scaleState = state;
		string text = "goingDown";
		string text2 = "goingUp";
		switch (scaleState)
		{
		case ScaleState.leftDown:
			ScaleLeft.animator.SetBool(text, value: true);
			ScaleLeft.animator.SetBool(text2, value: false);
			ScaleRight.animator.SetBool(text, value: false);
			ScaleRight.animator.SetBool(text2, value: true);
			ScalesUpSound();
			ScalesDownSound();
			break;
		case ScaleState.rightDown:
			ScaleLeft.animator.SetBool(text, value: false);
			ScaleLeft.animator.SetBool(text2, value: true);
			ScaleRight.animator.SetBool(text, value: true);
			ScaleRight.animator.SetBool(text2, value: false);
			ScalesUpSound();
			ScalesDownSound();
			break;
		case ScaleState.still:
			ScaleLeft.animator.SetBool(text, value: false);
			ScaleLeft.animator.SetBool(text2, value: false);
			ScaleRight.animator.SetBool(text, value: false);
			ScaleRight.animator.SetBool(text2, value: false);
			if (ScaleLeft.animator.GetCurrentAnimatorStateInfo(0).IsName("Scale_Idle"))
			{
				AudioManager.Stop("castle_scales_tip_up");
				AudioManager.Stop("castle_scales_tip_down");
			}
			break;
		}
	}

	private void ScalesUpSound()
	{
		if (!AudioManager.CheckIfPlaying("castle_scales_tip_up"))
		{
			AudioManager.Play("castle_scales_tip_up");
			emitAudioFromObject.Add("castle_scales_tip_up");
		}
	}

	private void ScalesDownSound()
	{
		if (!AudioManager.CheckIfPlaying("castle_scales_tip_down"))
		{
			AudioManager.Play("castle_scales_tip_down");
			emitAudioFromObject.Add("castle_scales_tip_down");
		}
	}
}
