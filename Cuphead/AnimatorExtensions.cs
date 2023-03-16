using System;
using System.Collections;
using UnityEngine;

public static class AnimatorExtensions
{
	public static float GetCurrentClipLength(this Animator animator, int layer = 0)
	{
		AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(layer);
		if (currentAnimatorClipInfo.Length == 0)
		{
			return 0f;
		}
		AnimationClip clip = currentAnimatorClipInfo[0].clip;
		return clip.length;
	}

	public static void FloorFrame(this Animator animator, int layer = 0)
	{
		roundFrame(animator, layer, Mathf.Floor);
	}

	public static void RoundFrame(this Animator animator, int layer = 0)
	{
		roundFrame(animator, layer, Mathf.Round);
	}

	private static void roundFrame(Animator animator, int layer, Func<float, float> rounder)
	{
		AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(layer);
		if (currentAnimatorClipInfo.Length != 0)
		{
			AnimationClip clip = currentAnimatorClipInfo[0].clip;
			float frameRate = clip.frameRate;
			float length = clip.length;
			float normalizedTime = animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
			float arg = normalizedTime * length * frameRate;
			float normalizedTime2 = rounder(arg) / frameRate / length;
			animator.Play(0, layer, normalizedTime2);
		}
	}

	public static Coroutine WaitForAnimationToStart(this Animator animator, MonoBehaviour parent, string animationName, bool waitForEndOfFrame = false)
	{
		return animator.WaitForAnimationToStart(parent, animationName, 0, waitForEndOfFrame);
	}

	public static Coroutine WaitForAnimationToStart(this Animator animator, MonoBehaviour parent, string animationName, int layer, bool waitForEndOfFrame = false)
	{
		int animationHash = Animator.StringToHash(animator.GetLayerName(layer) + "." + animationName);
		return animator.WaitForAnimationToStart(parent, animationHash, layer, waitForEndOfFrame);
	}

	public static Coroutine WaitForAnimationToStart(this Animator animator, MonoBehaviour parent, int animationHash, int layer, bool waitForEndOfFrame = false)
	{
		return parent.StartCoroutine(waitForAnimStart_cr(animator, layer, animationHash, waitForEndOfFrame));
	}

	private static IEnumerator waitForAnimStart_cr(Animator animator, int layer, int animationHash, bool waitForEndOfFrame)
	{
		while (animator.GetCurrentAnimatorStateInfo(layer).fullPathHash != animationHash)
		{
			if (waitForEndOfFrame)
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				yield return null;
			}
		}
	}

	public static Coroutine WaitForAnimationToEnd(this Animator animator, MonoBehaviour parent, bool waitForEndOfFrame = false)
	{
		return parent.StartCoroutine(waitForAnimEnd_cr(parent, animator, 0, waitForEndOfFrame));
	}

	private static IEnumerator waitForAnimEnd_cr(MonoBehaviour parent, Animator animator, int layer, bool waitForEndOfFrame)
	{
		int current = animator.GetCurrentAnimatorStateInfo(layer).fullPathHash;
		while (current == animator.GetCurrentAnimatorStateInfo(layer).fullPathHash)
		{
			if (waitForEndOfFrame)
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				yield return null;
			}
		}
	}

	public static Coroutine WaitForAnimationToEnd(this Animator animator, MonoBehaviour parent, string name, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		return animator.WaitForAnimationToEnd(parent, name, 0, waitForEndOfFrame, waitForStart);
	}

	public static Coroutine WaitForAnimationToEnd(this Animator animator, MonoBehaviour parent, string name, int layer, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		int animationHash = Animator.StringToHash(animator.GetLayerName(layer) + "." + name);
		return animator.WaitForAnimationToEnd(parent, animationHash, layer, waitForEndOfFrame, waitForStart);
	}

	public static Coroutine WaitForAnimationToEnd(this Animator animator, MonoBehaviour parent, int animationHash, int layer = 0, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		return parent.StartCoroutine(waitForNamedAnimEnd_cr(parent, animator, animationHash, layer, waitForEndOfFrame, waitForStart));
	}

	private static IEnumerator waitForNamedAnimEnd_cr(MonoBehaviour parent, Animator animator, int animationHash, int layer, bool waitForEndOfFrame, bool waitForStart = true)
	{
		if (waitForStart)
		{
			yield return parent.StartCoroutine(waitForAnimStart_cr(animator, layer, animationHash, waitForEndOfFrame));
		}
		while (animator.GetCurrentAnimatorStateInfo(layer).fullPathHash == animationHash)
		{
			if (waitForEndOfFrame)
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				yield return null;
			}
		}
	}

	public static Coroutine WaitForNormalizedTime(this Animator animator, MonoBehaviour parent, float normalizedTime, string name = null, int layer = 0, bool allowEqualTime = false, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		int? animationHash = null;
		if (name != null)
		{
			animationHash = Animator.StringToHash(animator.GetLayerName(layer) + "." + name);
		}
		return animator.WaitForNormalizedTime(parent, normalizedTime, animationHash, layer, allowEqualTime, waitForEndOfFrame, waitForStart);
	}

	public static Coroutine WaitForNormalizedTime(this Animator animator, MonoBehaviour parent, float normalizedTime, int? animationHash, int layer = 0, bool allowEqualTime = false, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		return parent.StartCoroutine(waitForNormalizedTime_cr(parent, animator, normalizedTime, animationHash, layer, allowEqualTime, waitForEndOfFrame, waitForStart, looping: false));
	}

	public static Coroutine WaitForNormalizedTimeLooping(this Animator animator, MonoBehaviour parent, float normalizedTimeDecimal, string name = null, int layer = 0, bool allowEqualTime = false, bool waitForEndOfFrame = false, bool waitForStart = true)
	{
		int? animationHash = null;
		if (name != null)
		{
			animationHash = Animator.StringToHash(animator.GetLayerName(layer) + "." + name);
		}
		return parent.StartCoroutine(waitForNormalizedTime_cr(parent, animator, normalizedTimeDecimal, animationHash, layer, allowEqualTime, waitForEndOfFrame, waitForStart, looping: true));
	}

	private static IEnumerator waitForNormalizedTime_cr(MonoBehaviour parent, Animator animator, float normalizedTime, int? animationHash, int layer, bool allowEqualTime, bool waitForEndOfFrame, bool waitForStart, bool looping)
	{
		if (animationHash.HasValue && waitForStart)
		{
			yield return parent.StartCoroutine(waitForAnimStart_cr(animator, layer, animationHash.Value, waitForEndOfFrame));
		}
		int target = ((!animationHash.HasValue) ? animator.GetCurrentAnimatorStateInfo(layer).fullPathHash : animationHash.Value);
		while (true)
		{
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
			float t = ((!looping) ? stateInfo.normalizedTime : MathUtilities.DecimalPart(stateInfo.normalizedTime));
			if (((!allowEqualTime) ? (stateInfo.normalizedTime >= normalizedTime) : (stateInfo.normalizedTime > normalizedTime)) || stateInfo.fullPathHash != target)
			{
				break;
			}
			if (waitForEndOfFrame)
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				yield return null;
			}
		}
	}
}
