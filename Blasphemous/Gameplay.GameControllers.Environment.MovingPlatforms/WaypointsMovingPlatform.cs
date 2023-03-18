using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.MovingPlatforms;

[RequireComponent(typeof(Collider2D))]
public class WaypointsMovingPlatform : MonoBehaviour, IActionable
{
	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	private string MovingPlatformLoop;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	private string MovingPlatformStop;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private List<WaypointsSection> waypointsSections;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[PropertyRange(0.0, 60.0)]
	private float RunningTimeout;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[PropertyRange(0.0, 60.0)]
	private float StartingTimeout;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool StartRunning;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool StartRunningWhenFirstStepped;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool StopAtEnd;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool OneWayOnly;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool DeactivatesGameobjectsWhenStepped;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool DeactivatesGameobjectsWhenRunning;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("ShowGameobjectsToDeactivate", true)]
	private List<GameObject> GameobjectsToDeactivate;

	[SerializeField]
	[BoxGroup("Flag Settings", true, false, 0)]
	[OnValueChanged("FixFlagName", false)]
	private string OnChosenWaypoint;

	[SerializeField]
	[BoxGroup("Flag Settings", true, false, 0)]
	[ValueDropdown("ListCurrentIndexes")]
	private readonly int IndexOfTheChosenWaypoint;

	private const float DistanceThreshold = 0.01f;

	private SpriteRenderer _spriteRenderer;

	private Collider2D _platformCollider;

	private EventInstance _movementLoopFx;

	private Tweener _horizontalTweener;

	private Tweener _verticalTweener;

	private Vector3 _origin;

	private Vector3 _destination;

	private int _currentSectionIndex;

	private float _speed;

	private Ease _horizontalEase;

	private Ease _verticalEase;

	private bool _running;

	private bool _endArrived;

	private bool _waypointsDetached;

	private bool _tpoHasSteppedThis;

	private float currentRunningTimeoutLapse;

	private float currentStartingTimeoutLapse;

	private bool ShowGameobjectsToDeactivate => DeactivatesGameobjectsWhenStepped || DeactivatesGameobjectsWhenRunning;

	public bool Enabled { get; private set; }

	private bool RunningTimeoutConsumed => currentRunningTimeoutLapse <= 0f;

	private bool StartingTimeoutConsumed => currentStartingTimeoutLapse <= 0f;

	public bool Locked { get; set; }

	private void Start()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_platformCollider = GetComponent<BoxCollider2D>();
		if (StartRunning)
		{
			_running = true;
		}
		_currentSectionIndex = (Core.Events.GetFlag(OnChosenWaypoint) ? IndexOfTheChosenWaypoint : 0);
		SetSectionData();
		currentRunningTimeoutLapse = RunningTimeout;
		currentStartingTimeoutLapse = StartingTimeout;
		base.transform.position = _origin;
	}

	private void Update()
	{
		if (!_platformCollider.enabled || !_running)
		{
			return;
		}
		if (DeactivatesGameobjectsWhenRunning && !_tpoHasSteppedThis)
		{
			foreach (GameObject item in GameobjectsToDeactivate)
			{
				if ((bool)item)
				{
					item.SetActive(!DOTween.IsTweening(base.transform));
				}
			}
		}
		currentStartingTimeoutLapse -= Time.deltaTime;
		if (!StartingTimeoutConsumed)
		{
			return;
		}
		if (Core.Logic.IsPaused)
		{
			if (_horizontalTweener != null && _horizontalTweener.IsPlaying() && _verticalTweener != null && _verticalTweener.IsPlaying())
			{
				_horizontalTweener.Pause();
				_verticalTweener.Pause();
				return;
			}
		}
		else if (_horizontalTweener != null && !_horizontalTweener.IsPlaying() && _verticalTweener != null && !_verticalTweener.IsPlaying())
		{
			_horizontalTweener.TogglePause();
			_verticalTweener.TogglePause();
		}
		float num = Vector2.Distance(base.transform.position, _destination);
		float duration = num / _speed;
		currentRunningTimeoutLapse -= Time.deltaTime;
		if (!DOTween.IsTweening(base.transform) && RunningTimeoutConsumed && num > 0f)
		{
			if (!_waypointsDetached)
			{
				DetachWaypoints();
			}
			_horizontalTweener = base.transform.DOMoveX(_destination.x, duration).SetEase(_horizontalEase);
			_verticalTweener = base.transform.DOMoveY(_destination.y, duration).SetEase(_verticalEase);
			PlayMovementLoopFx(ref _movementLoopFx);
		}
		if (!(num <= 0.01f))
		{
			return;
		}
		if (_currentSectionIndex == waypointsSections.Count - 1)
		{
			if (StopAtEnd)
			{
				Stop();
				return;
			}
			_currentSectionIndex = 0;
			SetSectionData();
		}
		else
		{
			_currentSectionIndex++;
			SetSectionData();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			if (_horizontalTweener != null && _horizontalTweener.IsPlaying() && _verticalTweener != null && _verticalTweener.IsPlaying())
			{
				_horizontalTweener.TogglePause();
				_verticalTweener.TogglePause();
			}
		}
		else if (_horizontalTweener != null && _horizontalTweener.IsPlaying() && _verticalTweener != null && _verticalTweener.IsPlaying())
		{
			_horizontalTweener.Pause();
			_verticalTweener.Pause();
		}
	}

	private void DetachWaypoints()
	{
		_waypointsDetached = true;
		foreach (WaypointsSection waypointsSection in waypointsSections)
		{
			waypointsSection.StartingPoint.parent = null;
			waypointsSection.EndingPoint.parent = null;
		}
	}

	private void AttachWaypoints()
	{
		_waypointsDetached = false;
		foreach (WaypointsSection waypointsSection in waypointsSections)
		{
			waypointsSection.StartingPoint.parent = base.gameObject.transform;
			waypointsSection.EndingPoint.parent = base.gameObject.transform;
		}
	}

	private void SetSectionData()
	{
		_speed = waypointsSections[_currentSectionIndex].speed;
		_horizontalEase = waypointsSections[_currentSectionIndex].horizontalEase;
		_verticalEase = waypointsSections[_currentSectionIndex].verticalEase;
		Transform startingPoint = waypointsSections[_currentSectionIndex].StartingPoint;
		Transform endingPoint = waypointsSections[_currentSectionIndex].EndingPoint;
		_origin = new Vector2(startingPoint.position.x, startingPoint.position.y);
		_destination = new Vector2(endingPoint.position.x, endingPoint.position.y);
		if (_currentSectionIndex > 0)
		{
			currentStartingTimeoutLapse = waypointsSections[_currentSectionIndex - 1].waitTimeAtDestination;
		}
		else
		{
			currentStartingTimeoutLapse = waypointsSections[waypointsSections.Count - 1].waitTimeAtDestination;
		}
	}

	private void Stop()
	{
		currentRunningTimeoutLapse = RunningTimeout;
		currentStartingTimeoutLapse = StartingTimeout;
		base.transform.position = _destination;
		_destination = _origin;
		_origin = base.transform.position;
		StopMovementLoopFx(ref _movementLoopFx);
		PlayMovementStopFx();
		_horizontalTweener.Kill();
		_verticalTweener.Kill();
		_horizontalTweener = null;
		_verticalTweener = null;
		if (OneWayOnly)
		{
			_endArrived = true;
			_running = false;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Penitent"))
		{
			Core.Logic.Penitent.FloorChecker.OnMovingPlatform = true;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!other.gameObject.CompareTag("Penitent") || Core.Logic.Penitent.IsGrabbingCliffLede || Core.Logic.Penitent.IsClimbingCliffLede)
		{
			return;
		}
		_tpoHasSteppedThis = true;
		if (DeactivatesGameobjectsWhenStepped)
		{
			foreach (GameObject item in GameobjectsToDeactivate)
			{
				if ((bool)item)
				{
					item.SetActive(value: false);
				}
			}
		}
		if (StartRunningWhenFirstStepped && !_running)
		{
			Use();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Penitent"))
		{
			Core.Logic.Penitent.FloorChecker.OnMovingPlatform = false;
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying || waypointsSections.Count <= 0)
		{
			return;
		}
		foreach (WaypointsSection waypointsSection in waypointsSections)
		{
			if ((bool)waypointsSection.StartingPoint && (bool)waypointsSection.EndingPoint)
			{
				Debug.DrawLine(waypointsSection.StartingPoint.position, waypointsSection.EndingPoint.position);
			}
		}
	}

	private void FixFlagName(string flagName)
	{
		OnChosenWaypoint = flagName.Replace(' ', '_').ToUpper();
	}

	private IList<ValueDropdownItem<int>> ListCurrentIndexes()
	{
		ValueDropdownList<int> valueDropdownList = new ValueDropdownList<int>();
		for (int i = 0; i < waypointsSections.Count; i++)
		{
			valueDropdownList.Add(i);
		}
		return valueDropdownList;
	}

	private void SetMotionFlags()
	{
		bool b = base.transform.position == waypointsSections[IndexOfTheChosenWaypoint].StartingPoint.position;
		Core.Events.SetFlag(OnChosenWaypoint, b);
	}

	public void ResetPlatform()
	{
		_running = false;
		if (DOTween.IsTweening(base.transform))
		{
			StopMovementLoopFx(ref _movementLoopFx);
			DOTween.Kill(base.transform);
		}
		_currentSectionIndex = (Core.Events.GetFlag(OnChosenWaypoint) ? IndexOfTheChosenWaypoint : 0);
		SetSectionData();
		currentRunningTimeoutLapse = RunningTimeout;
		currentStartingTimeoutLapse = StartingTimeout;
		base.transform.position = _origin;
		if (OneWayOnly)
		{
			_endArrived = false;
		}
		if (_waypointsDetached)
		{
			AttachWaypoints();
		}
		_tpoHasSteppedThis = false;
		foreach (GameObject item in GameobjectsToDeactivate)
		{
			if ((bool)item)
			{
				item.SetActive(value: true);
			}
		}
	}

	public Vector2 GetOrigin()
	{
		return _origin;
	}

	public Vector2 GetDestination()
	{
		return _destination;
	}

	public void Use()
	{
		if (!OneWayOnly || !_endArrived)
		{
			if (!_running)
			{
				SetSectionData();
				currentRunningTimeoutLapse = RunningTimeout;
				currentStartingTimeoutLapse = StartingTimeout;
			}
			_running = !_running;
			if (DOTween.IsTweening(base.transform))
			{
				StopMovementLoopFx(ref _movementLoopFx);
				DOTween.Kill(base.transform);
			}
		}
	}

	private void OnDestroy()
	{
		StopMovementLoopFx(ref _movementLoopFx);
	}

	private void PlayMovementStopFx()
	{
		if (!MovingPlatformStop.IsNullOrWhitespace() && _spriteRenderer.isVisible)
		{
			Core.Audio.PlaySfx(MovingPlatformStop);
		}
	}

	private void PlayMovementLoopFx(ref EventInstance audioInstance)
	{
		if (!MovingPlatformLoop.IsNullOrWhitespace())
		{
			if (_movementLoopFx.isValid())
			{
				_movementLoopFx.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				_movementLoopFx.release();
				_movementLoopFx = default(EventInstance);
			}
			_movementLoopFx = Core.Audio.CreateEvent(MovingPlatformLoop, base.transform.position);
			_movementLoopFx.start();
		}
	}

	private void StopMovementLoopFx(ref EventInstance audioInstance)
	{
		if (_movementLoopFx.isValid())
		{
			PlayMovementStopFx();
			_movementLoopFx.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_movementLoopFx.release();
			_movementLoopFx = default(EventInstance);
		}
	}
}
