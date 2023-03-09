using UnityEngine;

public class ChessQueenLevelCannonball : BasicProjectile
{
	private const float FRAME_TIME = 1f / 24f;

	private Vector3 vel;

	[SerializeField]
	private float minScale = 0.75f;

	[SerializeField]
	private SpriteRenderer sprite;

	private float frameTimer;

	private bool hit;

	protected override void Awake()
	{
		base.Awake();
		DamagesType.Player = false;
	}

	public override AbstractProjectile Create(Vector2 position, float rotation)
	{
		ChessQueenLevelCannonball chessQueenLevelCannonball = Create(position) as ChessQueenLevelCannonball;
		chessQueenLevelCannonball.vel = MathUtils.AngleToDirection(rotation);
		return chessQueenLevelCannonball;
	}

	protected override void Move()
	{
		if (!hit)
		{
			base.transform.position += vel * Speed * CupheadTime.FixedDelta;
			frameTimer += CupheadTime.FixedDelta;
			if (frameTimer >= 1f / 24f)
			{
				float num = Mathf.Lerp(1f, minScale, Mathf.InverseLerp(-150f, 440f, base.transform.position.y));
				sprite.transform.localScale = new Vector3(num, num);
				frameTimer -= 1f / 24f;
			}
		}
	}

	public void HitQueen()
	{
		hit = true;
		sprite.transform.localScale = new Vector3(1f, 1f);
		sprite.flipX = Rand.Bool();
		sprite.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		base.animator.Play("Explode");
		base.animator.Update(0f);
	}
}
