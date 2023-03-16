using System;
using System.Collections;
using UnityEngine;

public class ClownLevelAnimationManager : AbstractPausableComponent
{
	[SerializeField]
	private Animator headSprite;

	[SerializeField]
	private Transform balloonSprite;

	[SerializeField]
	private Transform pivotPoint;

	[SerializeField]
	private Animator[] twelveFpsAnimations;

	[SerializeField]
	private Animator[] twentyFourFpsAnimations;

	private bool invert;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		headSprite = headSprite.GetComponent<Animator>();
		pivotPoint.position = balloonSprite.position;
		StartCoroutine(head_cr());
		StartCoroutine(balloon_loop_cr());
		Animator[] array = twelveFpsAnimations;
		foreach (Animator ani in array)
		{
			StartCoroutine(manual_fps_animation_cr(ani, 1f / 12f));
		}
		Animator[] array2 = twentyFourFpsAnimations;
		foreach (Animator ani2 in array2)
		{
			StartCoroutine(manual_fps_animation_cr(ani2, 1f / 24f));
		}
	}

	private IEnumerator head_cr()
	{
		while (true)
		{
			float getSeconds = UnityEngine.Random.Range(3f, 8f);
			headSprite.SetTrigger("Continue");
			yield return CupheadTime.WaitForSeconds(this, getSeconds);
		}
	}

	private IEnumerator balloon_loop_cr()
	{
		float loopSize = 20f;
		float speed = 1f;
		float angle = 0f;
		while (true)
		{
			Vector3 pivotOffset = Vector3.left * 2f * loopSize;
			angle += speed * (float)CupheadTime.Delta;
			if (angle > (float)Math.PI * 2f)
			{
				invert = !invert;
				angle -= (float)Math.PI * 2f;
			}
			if (angle < 0f)
			{
				angle += (float)Math.PI * 2f;
			}
			float value;
			if (invert)
			{
				balloonSprite.position = pivotPoint.position + pivotOffset;
				value = 1f;
			}
			else
			{
				balloonSprite.position = pivotPoint.position;
				value = -1f;
			}
			Vector3 handleRotationX = new Vector3(Mathf.Cos(angle) * value * loopSize, 0f, 0f);
			Vector3 handleRotationY = new Vector3(0f, Mathf.Sin(angle) * loopSize, 0f);
			balloonSprite.position += handleRotationX + handleRotationY;
			yield return null;
		}
	}

	private IEnumerator manual_fps_animation_cr(Animator ani, float fps)
	{
		float frameTime = 0f;
		while (true)
		{
			frameTime += (float)CupheadTime.Delta;
			if (frameTime > fps)
			{
				frameTime -= fps;
				ani.enabled = true;
				ani.Update(fps);
				ani.enabled = false;
			}
			yield return null;
		}
	}
}
