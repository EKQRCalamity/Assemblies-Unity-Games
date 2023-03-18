using DG.Tweening;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Familiar.AI;

public class FamiliarBehaviour : MonoBehaviour
{
	public Enemy Target;

	public Vector2 PlayerOffsetPosition;

	public Vector2 CherubOffsetPosition;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float AmplitudeX = 10f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float AmplitudeY = 5f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float SpeedX = 1f;

	[FoldoutGroup("Floating Motion", true, 0)]
	public float SpeedY = 2f;

	[FoldoutGroup("Chasing Options", true, 0)]
	public float CherubCriticalDistance = 4f;

	public const string CherubName = "CollectibleCherubCaptor";

	[FoldoutGroup("Chasing", true, 0)]
	public float ChasingElongation = 0.5f;

	[FoldoutGroup("Chasing", true, 0)]
	public float ChasingSpeed = 5f;

	private Vector3 _velocity = Vector3.zero;

	private float _index;

	private float _currentAmplitudeY;

	private float _currentAmplitudeX;

	public Familiar Familiar { get; private set; }

	public Enemy CherubInstance { get; private set; }

	public Vector3 ChaseVelocity => _velocity;

	private void Awake()
	{
		Familiar = GetComponent<Familiar>();
	}

	private void Start()
	{
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeX = x;
		}, _currentAmplitudeX, AmplitudeX, 1f);
		DOTween.To(delegate(float x)
		{
			_currentAmplitudeY = x;
		}, _currentAmplitudeY, AmplitudeY, 1f);
		Familiar.StateMachine.SwitchState<FamiliarChasePlayerState>();
		Invoke("LookForCherub", 3f);
	}

	public void ChasingEntity(Entity entity, Vector2 offset)
	{
		Vector3 chasingTargetPosition = GetChasingTargetPosition(entity, offset);
		chasingTargetPosition.y = entity.transform.position.y + offset.y;
		Familiar.transform.position = Vector3.SmoothDamp(Familiar.transform.position, chasingTargetPosition, ref _velocity, ChasingElongation, ChasingSpeed);
	}

	private Vector3 GetChasingTargetPosition(Entity entity, Vector2 offset)
	{
		Vector3 position = entity.transform.position;
		if (entity.Status.Orientation == EntityOrientation.Left)
		{
			position.x += offset.x;
		}
		else
		{
			position.x -= offset.x;
		}
		return position;
	}

	public void SetOrientationByVelocity(Vector3 velocity)
	{
		if (velocity.x < 0f)
		{
			Familiar.SetOrientation(EntityOrientation.Left);
		}
		else if (velocity.x > 0f)
		{
			Familiar.SetOrientation(EntityOrientation.Right);
		}
	}

	public void Floating()
	{
		_index += Time.deltaTime;
		float x = _currentAmplitudeX * Mathf.Sin(SpeedX * _index);
		float y = Mathf.Cos(SpeedY * _index) * _currentAmplitudeY;
		Familiar.SpriteRenderer.transform.localPosition = new Vector3(x, y, 0f);
	}

	private void LookForCherub()
	{
		if (FindCherub())
		{
			PursueCherub();
		}
	}

	private bool FindCherub()
	{
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		bool result = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name.Contains("CollectibleCherubCaptor"))
			{
				Debug.Log("cherub name: " + array[i].name);
				result = true;
				CherubInstance = array[i];
				CherubInstance.OnDeath += OnCherubRescued;
				break;
			}
		}
		return result;
	}

	private void PursueCherub()
	{
		Familiar.StateMachine.SwitchState<FamiliarChaseCherubState>();
	}

	private void OnCherubRescued()
	{
		CherubInstance.OnDeath -= OnCherubRescued;
		CherubInstance = null;
		Familiar.StateMachine.SwitchState<FamiliarChasePlayerState>();
	}

	private void OnDestroy()
	{
		if (CherubInstance != null && !CherubInstance.Status.Dead)
		{
			CherubInstance.OnDeath -= OnCherubRescued;
		}
	}
}
