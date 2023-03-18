using UnityEngine;

namespace Gameplay.GameControllers.Environment;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Vegetation : MonoBehaviour
{
	protected Animator plantAnimator;

	protected BoxCollider2D plantCollider;

	protected bool isShaking;

	public abstract void Shaking(float playTimeAnimation);
}
