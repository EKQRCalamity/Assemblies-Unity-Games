using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Ladder;

[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : MonoBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private float _deltaRecoverTime;

	private BoxCollider2D _ladderBoxCollider;

	private const float RecoverTime = 0.5f;

	private void Start()
	{
		_penitent = Core.Logic.Penitent;
		_ladderBoxCollider = GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		SetLadderClimbable();
	}

	protected void SetLadderClimbable()
	{
		if (_ladderBoxCollider == null)
		{
			Log.Error("Ladder null collider.");
			return;
		}
		if (_penitent != null && _penitent.IsJumpingOff)
		{
			_deltaRecoverTime = 0f;
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			if (_ladderBoxCollider.enabled)
			{
				_ladderBoxCollider.enabled = false;
			}
			return;
		}
		_deltaRecoverTime += Time.deltaTime;
		if (_deltaRecoverTime >= 0.5f)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Ladder");
			if (!_ladderBoxCollider.enabled)
			{
				_ladderBoxCollider.enabled = true;
			}
		}
	}
}
