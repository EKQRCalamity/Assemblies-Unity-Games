using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Audio;

public abstract class EnemyMovementSetAudio : MonoBehaviour
{
	protected FMODAudioManager audioManager;

	public LayerMask floorLayerMask;

	public abstract void SetFxIdByFloor(string floorTag);
}
