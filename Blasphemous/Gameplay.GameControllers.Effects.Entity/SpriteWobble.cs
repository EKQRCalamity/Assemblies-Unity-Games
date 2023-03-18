using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Entity;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteWobble : Ability
{
	[SerializeField]
	protected SpriteRenderer SpriteRenderer;

	public float Amplitude = 1.75f;

	public float Speed = 40f;

	public float TimeWobbling;

	private float _currentTimeWobbling;

	private Vector3 _startPosition;

	protected override void OnAwake()
	{
		base.OnAwake();
		if (SpriteRenderer == null)
		{
			Debug.LogError("This component needs a sprite renderer component");
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_currentTimeWobbling -= Time.deltaTime;
		if (_currentTimeWobbling > 0f)
		{
			Wobbling();
		}
		else if (base.Casting)
		{
			StopCast();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		_currentTimeWobbling = TimeWobbling;
		_startPosition = new Vector3(base.transform.position.x, base.transform.position.y);
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		base.transform.position = _startPosition;
	}

	private void Wobbling()
	{
		Vector3 position = base.transform.position;
		position.x += Mathf.Sin(Time.time * Speed) * Time.deltaTime * Amplitude;
		base.transform.position = position;
	}
}
