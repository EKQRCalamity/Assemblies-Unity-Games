using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Audio;

public abstract class EnemyAttackAudio : MonoBehaviour
{
	protected FMODAudioManager audioManager;

	public abstract void SetFxIdBySurface(string surface);
}
