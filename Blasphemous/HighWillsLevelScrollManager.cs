using Gameplay.GameControllers.Camera;
using UnityEngine;

public class HighWillsLevelScrollManager : MonoBehaviour
{
	public BoxCollider2D Collider;

	public CameraNumericBoundaries PrevCamNumBound;

	public CameraNumericBoundaries ScrollCamNumBound;

	public float Speed = 1f;

	private bool _stopped;

	private bool _scrollActive;

	private Vector2 _startingCamBoundaries;

	public void Reset()
	{
		if (!_stopped)
		{
			_scrollActive = false;
			ScrollCamNumBound.LeftBoundary = _startingCamBoundaries.x;
			ScrollCamNumBound.RightBoundary = _startingCamBoundaries.y;
			PrevCamNumBound.SetBoundaries();
		}
	}

	public void Stop()
	{
		_stopped = true;
		_scrollActive = false;
		ScrollCamNumBound.LeftBoundary = _startingCamBoundaries.x;
		ScrollCamNumBound.RightBoundary = _startingCamBoundaries.y;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent") && !_scrollActive)
		{
			Activate();
		}
	}

	private void Activate()
	{
		_startingCamBoundaries = new Vector2(ScrollCamNumBound.LeftBoundary, ScrollCamNumBound.RightBoundary);
		_scrollActive = true;
	}

	private void LateUpdate()
	{
		if (_scrollActive)
		{
			float num = Time.deltaTime * Speed;
			ScrollCamNumBound.LeftBoundary += num;
			ScrollCamNumBound.RightBoundary += num;
			ScrollCamNumBound.SetBoundaries();
		}
	}
}
