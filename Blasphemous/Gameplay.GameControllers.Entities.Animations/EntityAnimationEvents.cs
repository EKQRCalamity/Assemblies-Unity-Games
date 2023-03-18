using Framework.Managers;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Animations;

public class EntityAnimationEvents : MonoBehaviour
{
	protected LevelInitializer currentLevel;

	public bool AnimationsFXReady { get; set; }

	private void Start()
	{
		OnStart();
	}

	protected virtual void OnStart()
	{
		currentLevel = Core.Logic.CurrentLevelConfig;
	}

	private void Update()
	{
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{
	}

	public virtual void Rebound()
	{
	}
}
