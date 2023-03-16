using UnityEngine;

public class KitchenParallaxLayer : ParallaxLayer
{
	[SerializeField]
	private float startY;

	[SerializeField]
	private float endY;

	[SerializeField]
	private bool ignoreX;

	private SaltbakerLevel level;

	protected override void Start()
	{
		base.Start();
		level = Level.Current as SaltbakerLevel;
	}

	protected override void UpdateMinMax()
	{
		Vector3 position = base.transform.position;
		if (!ignoreX)
		{
			Vector2 vector = _camera.transform.position;
			Vector2 zero = Vector2.zero;
			float num = vector.x + Mathf.Abs(_camera.Left);
			float num2 = _camera.Right + Mathf.Abs(_camera.Left);
			zero.x = num / num2;
			if (float.IsNaN(zero.x))
			{
				zero.x = 0.5f;
			}
			position.x = Mathf.Lerp(bottomLeft.x, topRight.x, zero.x) + _camera.transform.position.x;
		}
		position.y = Mathf.Lerp(startY, endY, level.yScrollPos);
		base.transform.position = position;
	}
}
