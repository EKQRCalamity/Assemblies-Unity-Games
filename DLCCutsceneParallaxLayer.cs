using UnityEngine;

public class DLCCutsceneParallaxLayer : AbstractPausableComponent
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

	protected AbstractCupheadCamera _camera;

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
		_camera = CupheadCutsceneCamera.Current;
		_startPosition = base.transform.position;
		_cameraStartPosition = _camera.transform.position;
	}

	private void LateUpdate()
	{
		Type type = this.type;
		if (type == Type.Comparative || type != Type.Centered)
		{
			UpdateComparative();
		}
		else
		{
			UpdateCentered();
		}
	}

	protected virtual void UpdateComparative()
	{
		Vector3 position = base.transform.position;
		position.x = _offset.x + _camera.transform.position.x * percentage;
		position.y = _offset.y + _camera.transform.position.y * percentage;
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
