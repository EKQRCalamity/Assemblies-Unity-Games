using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses;

[Serializable]
public class BossPhase
{
	public string Id = string.Empty;

	[MinMaxSlider(0f, 100f, true)]
	public Vector2 PhaseInterval;

	public bool IsActive { get; set; }
}
