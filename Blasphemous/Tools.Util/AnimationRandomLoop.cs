using UnityEngine;

namespace Tools.Util;

[RequireComponent(typeof(Animator))]
public class AnimationRandomLoop : MonoBehaviour
{
	public string initialState;

	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		bool flag = anim.HasState(0, Animator.StringToHash(initialState));
		if ((bool)anim && flag)
		{
			anim.Play(initialState, 0, Random.Range(0f, 1f));
		}
	}
}
