using UnityEngine;

namespace Gameplay.GameControllers.Entities.Animations;

[RequireComponent(typeof(Animator))]
public class EnemyAnimatorInyector : MonoBehaviour
{
	public enum BooleanParameter
	{
		True,
		False
	}

	[SerializeField]
	protected Entity OwnerEntity;

	public Animator EntityAnimator { get; protected set; }

	private void Awake()
	{
		if (OwnerEntity == null)
		{
			Debug.LogError("Any or all of the properties have not been initialized");
		}
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	private void Start()
	{
		if (OwnerEntity != null)
		{
			EntityAnimator = OwnerEntity.Animator;
		}
		OnStart();
	}

	protected virtual void OnStart()
	{
	}

	private void Update()
	{
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{
	}

	public void SetVulnerable(BooleanParameter parameter)
	{
		Enemy enemy = (Enemy)OwnerEntity;
		if (!(enemy == null))
		{
			enemy.IsVulnerable = parameter == BooleanParameter.True;
		}
	}
}
