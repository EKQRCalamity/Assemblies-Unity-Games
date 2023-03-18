using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Managers.LevelSingleton.InvertedBell;

public class BadajoManager : MonoBehaviour
{
	public Animator Animator;

	private Material mat;

	public List<GameObject> leftChainStuff;

	public List<GameObject> rightChainStuff;

	private void Awake()
	{
		mat = GetComponent<MeshRenderer>().material;
	}

	private void Start()
	{
	}

	private void CheckFlags()
	{
		if (Core.Events.GetFlag("BELL_PUZZLE1_ACTIVATED"))
		{
			Debug.Log(" PUZZLE 1 ACTIVATED");
			foreach (GameObject item in leftChainStuff)
			{
				item.SetActive(value: false);
			}
		}
		if (!Core.Events.GetFlag("BELL_PUZZLE2_ACTIVATED"))
		{
			return;
		}
		Debug.Log(" PUZZLE 2 ACTIVATED");
		foreach (GameObject item2 in rightChainStuff)
		{
			item2.SetActive(value: false);
		}
	}

	public void PlayFeedback()
	{
	}

	public void PlayReactionRight()
	{
		Animator.Play("REACTION_RIGHT");
	}

	public void PlayReactionLeft()
	{
		Animator.Play("REACTION_LEFT");
	}

	public void PlayBreak()
	{
		Animator.Play("BREAK");
		StartCoroutine(LerpMatValue(3f, Callback));
	}

	private void Callback()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(5f, new Vector3(2f, 2f, 0f), 100, 0.25f, 0f, default(Vector3), 0.06f, ignoreTimeScale: true);
	}

	private IEnumerator LerpMatValue(float maxSeconds, Action callback)
	{
		float counter = 0f;
		while (counter < maxSeconds)
		{
			float v = Mathf.Lerp(1f, 0.3f, counter / maxSeconds);
			mat.SetFloat("_Multiply", v);
			counter += Time.deltaTime;
			yield return null;
		}
		callback();
	}
}
