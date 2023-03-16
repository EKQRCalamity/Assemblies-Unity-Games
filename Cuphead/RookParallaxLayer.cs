using UnityEngine;

public class RookParallaxLayer : ParallaxLayer
{
	[SerializeField]
	private float yModifier = 0.5f;

	protected override void UpdateComparative()
	{
		Vector3 position = base.transform.position;
		position.x = base._offset.x + _camera.transform.position.x * percentage;
		position.y = base._offset.y + _camera.transform.position.y * percentage * yModifier;
		base.transform.position = position;
	}
}
