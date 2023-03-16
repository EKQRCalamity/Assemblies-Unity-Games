using UnityEngine;

public class ParallaxLayer : AbstractPausableComponent
{
	public enum Type
	{
		MinMax,
		Comparative,
		Centered
	}

	public Type type;

	[Range(-3f, 3f)]
	public float percentage;

	public Vector2 bottomLeft;

	public Vector2 topRight;

	protected CupheadLevelCamera _camera;

	private bool _initialized;

	private Vector3 _startPosition;

	private Vector3 _cameraStartPosition;

	[SerializeField]
	private bool overrideCameraRange;

	[SerializeField]
	private MinMax overrideCameraX;

	[SerializeField]
	private MinMax overrideCameraY;

	protected Vector2 _offset => _startPosition - _cameraStartPosition;

	protected virtual void Start()
	{
		_camera = CupheadLevelCamera.Current;
		_startPosition = base.transform.position;
		_cameraStartPosition = _camera.transform.position;
	}

	private void LateUpdate()
	{
		switch (type)
		{
		default:
			UpdateComparative();
			break;
		case Type.MinMax:
			UpdateMinMax();
			break;
		case Type.Centered:
			UpdateCentered();
			break;
		}
	}

	protected virtual void UpdateComparative()
	{
		Vector3 position = base.transform.position;
		position.x = _offset.x + _camera.transform.position.x * percentage;
		position.y = _offset.y + _camera.transform.position.y * percentage;
		base.transform.position = position;
	}

	protected virtual void UpdateMinMax()
	{
		Vector3 position = base.transform.position;
		Vector2 vector = _camera.transform.position;
		Vector2 zero = Vector2.zero;
		float num = vector.x + Mathf.Abs(_camera.Left);
		float num2 = _camera.Right + Mathf.Abs(_camera.Left);
		float num3 = vector.y + Mathf.Abs(_camera.Bottom);
		float num4 = _camera.Top + Mathf.Abs(_camera.Bottom);
		if (overrideCameraRange)
		{
			num = vector.x + Mathf.Abs(overrideCameraX.min);
			num3 = vector.y + Mathf.Abs(overrideCameraY.min);
			num2 = overrideCameraX.max - overrideCameraX.min;
			num4 = overrideCameraY.max - overrideCameraY.min;
		}
		zero.x = num / num2;
		zero.y = num3 / num4;
		if (float.IsNaN(zero.x))
		{
			zero.x = 0.5f;
		}
		if (float.IsNaN(zero.y))
		{
			zero.y = 0.5f;
		}
		position.x = Mathf.Lerp(bottomLeft.x, topRight.x, zero.x) + _camera.transform.position.x;
		position.y = Mathf.Lerp(bottomLeft.y, topRight.y, zero.y) + _camera.transform.position.y;
		base.transform.position = position;
	}

	private void UpdateCentered()
	{
		Vector3 position = base.transform.position;
		position.x = _startPosition.x + (_camera.transform.position.x - _startPosition.x) * percentage;
		position.y = _startPosition.y + (_camera.transform.position.y - _startPosition.y) * percentage;
		base.transform.position = position;
	}
}
