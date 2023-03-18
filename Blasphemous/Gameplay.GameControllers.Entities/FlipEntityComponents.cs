using System;
using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class FlipEntityComponents : Trait
{
	public GameObject[] EntityComponents;

	private EntityOrientation _currentOrientation;

	private EntityOrientation _prevOrientation;

	protected override void OnAwake()
	{
		base.OnAwake();
		_prevOrientation = base.EntityOwner.Status.Orientation;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_currentOrientation = base.EntityOwner.Status.Orientation;
		FlipComponents();
	}

	private void FlipComponents()
	{
		if (_prevOrientation != _currentOrientation)
		{
			switch (_currentOrientation)
			{
			case EntityOrientation.Right:
				SetXRotateScale(1f);
				_prevOrientation = EntityOrientation.Right;
				break;
			case EntityOrientation.Left:
				SetXRotateScale(-1f);
				_prevOrientation = EntityOrientation.Left;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void SetXRotateScale(float xRotateScale)
	{
		Vector3 localScale = new Vector3(xRotateScale, 1f, 0f);
		GameObject[] entityComponents = EntityComponents;
		foreach (GameObject gameObject in entityComponents)
		{
			gameObject.transform.localScale = localScale;
		}
	}
}
