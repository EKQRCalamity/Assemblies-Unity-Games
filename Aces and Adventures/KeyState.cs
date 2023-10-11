using System;
using UnityEngine;

public class KeyState
{
	private static readonly KeyState Default = new KeyState();

	public bool isDown;

	public bool justPressed;

	public bool justReleased;

	public bool justClicked;

	public bool justDoubleClicked;

	public bool justHeld;

	public float timeHeld;

	public float timeHeldAtRelease;

	public float timeSinceLastClick;

	public float timeSinceLastRelease = float.MaxValue;

	public Vector2 dragVector;

	public float dragDistance;

	public bool isBeingDragged
	{
		get
		{
			if (isDown)
			{
				return dragDistance >= InputManager.I.DragDistanceThreshold;
			}
			return false;
		}
	}

	public float timeSincePressedOrReleased
	{
		get
		{
			if (!isDown)
			{
				return timeSinceLastRelease;
			}
			return 0f;
		}
	}

	public bool this[KState type]
	{
		get
		{
			switch (type)
			{
			case KState.Up:
				return !isDown;
			case KState.Down:
				return isDown;
			case KState.JustPressed:
				return justPressed;
			case KState.JustReleased:
				return justReleased;
			case KState.Clicked:
				if (justReleased)
				{
					return InputManager.I.IsClick(timeHeldAtRelease);
				}
				return false;
			case KState.ClickedWithoutDragging:
				return justClicked;
			case KState.DoubleClicked:
				return justDoubleClicked;
			case KState.JustHeld:
				return justHeld;
			case KState.Held:
				if (isDown)
				{
					return InputManager.I.IsHeld(timeHeld);
				}
				return false;
			case KState.JustReleasedHeld:
				if (justReleased)
				{
					return InputManager.I.IsHeld(timeHeldAtRelease);
				}
				return false;
			case KState.Dragged:
				return isBeingDragged;
			case KState.DraggedOrHeld:
				if (!this[KState.Dragged])
				{
					return this[KState.Held];
				}
				return true;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
		}
	}

	public bool this[KState checkType, KState? restrictType]
	{
		get
		{
			if (this[checkType])
			{
				if (restrictType.HasValue)
				{
					return !this[restrictType.Value];
				}
				return true;
			}
			return false;
		}
	}

	public KeyState(bool down = false)
	{
		Update(down);
	}

	public void Update(bool down)
	{
		bool flag = isBeingDragged;
		justPressed = down && !isDown;
		justReleased = !down && isDown;
		isDown = down;
		float num = timeHeld;
		if (justReleased)
		{
			timeHeldAtRelease = timeHeld;
			timeHeld = 0f;
			timeSinceLastRelease = 0f;
		}
		else if (down)
		{
			timeHeld += Time.unscaledDeltaTime;
			dragVector += InputManager.I.MousePosition.Project(AxisType.Z) - InputManager.I.LastMousePosition.Project(AxisType.Z);
			dragDistance = Math.Max(dragDistance, dragVector.magnitude);
		}
		else
		{
			timeSinceLastClick += Time.unscaledDeltaTime;
			timeSinceLastRelease += Time.unscaledDeltaTime;
			dragVector = Vector2.zero;
			dragDistance = 0f;
		}
		justHeld = InputManager.I.IsHeld(timeHeld) && !InputManager.I.IsHeld(num);
		justClicked = justReleased && InputManager.I.IsClick(timeHeldAtRelease) && !flag;
		justDoubleClicked = justClicked && InputManager.I.IsDoubleClick(timeSinceLastClick);
		if (justClicked)
		{
			timeSinceLastClick = 0f;
		}
	}

	public void Clear()
	{
		CopyFrom(Default);
	}

	public KeyState CopyFrom(KeyState copyFrom)
	{
		isDown = copyFrom.isDown;
		justPressed = copyFrom.justPressed;
		justReleased = copyFrom.justReleased;
		justClicked = copyFrom.justClicked;
		justDoubleClicked = copyFrom.justDoubleClicked;
		justHeld = copyFrom.justHeld;
		timeHeld = copyFrom.timeHeld;
		timeHeldAtRelease = copyFrom.timeHeldAtRelease;
		timeSinceLastClick = copyFrom.timeSinceLastClick;
		timeSinceLastRelease = copyFrom.timeSinceLastRelease;
		dragVector = copyFrom.dragVector;
		dragDistance = copyFrom.dragDistance;
		return this;
	}
}
