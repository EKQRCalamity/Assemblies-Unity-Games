using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class EnemySensor : MonoBehaviour
{
	private Entity entity;

	private bool reversePos;

	private void Start()
	{
		entity = GetComponentInParent<Entity>();
		reversePos = entity.Status.Orientation == EntityOrientation.Left;
		if (reversePos)
		{
			ReversePosition();
		}
		InheritedStart();
	}

	protected virtual void InheritedStart()
	{
	}

	private void Update()
	{
		if (entity.Status.Orientation == EntityOrientation.Left)
		{
			if (!reversePos)
			{
				reversePos = true;
				ReversePosition();
			}
		}
		else if (entity.Status.Orientation == EntityOrientation.Right && reversePos)
		{
			reversePos = !reversePos;
			ReversePosition();
		}
		InheritedUpdate();
	}

	protected virtual void InheritedUpdate()
	{
	}

	private void ReversePosition()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.y += 180f;
		base.transform.eulerAngles = eulerAngles;
	}
}
