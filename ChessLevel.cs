using System;
using UnityEngine;

public abstract class ChessLevel : Level
{
	[SerializeField]
	private LevelIntroAnimation levelIntroAnimation;

	private Mode originalMode;

	protected override void Awake()
	{
		originalMode = Level.CurrentMode;
		Level.SetCurrentMode(Mode.Normal);
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Level.SetCurrentMode(originalMode);
		levelIntroAnimation = null;
	}

	protected override LevelIntroAnimation CreateLevelIntro(Action callback)
	{
		return LevelIntroAnimation.CreateCustom(levelIntroAnimation, callback);
	}
}
