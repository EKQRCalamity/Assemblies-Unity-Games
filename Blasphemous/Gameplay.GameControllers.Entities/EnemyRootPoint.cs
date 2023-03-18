using System;
using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class EnemyRootPoint : MonoBehaviour
{
	public Enemy Enemy { get; private set; }

	public Vector2 LocalStartPosition { get; set; }

	private void Start()
	{
		Enemy = GetComponentInParent<Enemy>();
		LocalStartPosition = new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
		if (!(Enemy != null))
		{
			Debug.LogError("An Enemy component is needed!");
			base.enabled = false;
		}
	}

	private void Update()
	{
		SetPosition();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(base.transform.position, 0.1f);
	}

	private void SetPosition()
	{
		Vector3 localPosition = Enemy.Status.Orientation switch
		{
			EntityOrientation.Left => Vector3.Reflect(LocalStartPosition, Vector2.right), 
			EntityOrientation.Right => LocalStartPosition, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		base.transform.localPosition = localPosition;
	}
}
