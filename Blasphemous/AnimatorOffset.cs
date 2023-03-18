using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorOffset : MonoBehaviour
{
	private Animator _animator;

	public float minOffset = -0.2f;

	public float maxOffset = 0.1f;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_animator.speed = 1f + Random.Range(minOffset, maxOffset);
	}
}
