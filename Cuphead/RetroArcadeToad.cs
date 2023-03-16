using System.Collections;
using UnityEngine;

public class RetroArcadeToad : RetroArcadeEnemy
{
	private const float TOAD_MAX_X_POS = 330f;

	private const float OFFSET_Y = 50f;

	private const float OFFSCREEN_Y = 200f;

	private const float BASE_Y = 250f;

	private const float MOVE_Y_SPEED = 500f;

	private LevelProperties.RetroArcade.Toad properties;

	private RetroArcadeToadManager parent;

	private float gravity;

	private bool onLeft;

	public RetroArcadeToad Create(RetroArcadeToadManager parent, LevelProperties.RetroArcade.Toad properties, bool onLeft)
	{
		RetroArcadeToad retroArcadeToad = InstantiatePrefab<RetroArcadeToad>();
		retroArcadeToad.transform.SetPosition((!onLeft) ? 330f : (-330f), 200f);
		retroArcadeToad.properties = properties;
		retroArcadeToad.parent = parent;
		retroArcadeToad.hp = properties.hp;
		retroArcadeToad.onLeft = onLeft;
		return retroArcadeToad;
	}

	protected override void Start()
	{
		StartCoroutine(jump_cr());
	}

	private IEnumerator jump_cr()
	{
		float speedY = properties.jumpVerticalSpeedRange.RandomFloat();
		float speedX = properties.jumpHorizontalSpeedRange.RandomFloat();
		float velocityX = speedX;
		float velocityY = speedY;
		float ground = (float)Level.Current.Ground + 50f;
		bool jumping = false;
		bool goingUp = false;
		gravity = properties.jumpGravity;
		while (base.transform.position.y > ground)
		{
			velocityY -= gravity * (float)CupheadTime.Delta;
			base.transform.AddPosition(0f, velocityY * (float)CupheadTime.Delta);
			yield return null;
		}
		Vector3 pos = base.transform.position;
		pos.y = ground;
		base.transform.position = pos;
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, properties.jumpDelay.RandomFloat());
			velocityY = speedY;
			velocityX = ((!onLeft) ? (0f - speedX) : speedX);
			jumping = true;
			goingUp = true;
			while (jumping)
			{
				velocityY -= gravity * (float)CupheadTime.Delta;
				base.transform.AddPosition(velocityX * (float)CupheadTime.Delta, velocityY * (float)CupheadTime.Delta);
				if (velocityY < 0f && goingUp)
				{
					goingUp = false;
				}
				if (velocityY < 0f && jumping && base.transform.position.y <= ground)
				{
					jumping = false;
					pos = base.transform.position;
					pos.y = ground;
					base.transform.position = pos;
				}
				if ((base.transform.position.x < -330f && !onLeft) || (base.transform.position.x > 330f && onLeft))
				{
					if (onLeft)
					{
						base.transform.SetPosition(330f);
						onLeft = false;
					}
					else
					{
						base.transform.SetPosition(-330f);
						onLeft = true;
					}
					velocityX = ((!onLeft) ? (0f - speedX) : speedX);
				}
				yield return null;
			}
			yield return null;
		}
	}

	public override void Dead()
	{
		base.Dead();
		parent.OnToadDie();
	}
}
