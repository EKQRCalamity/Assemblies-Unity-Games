using System;
using UnityEngine;

[Serializable]
public class Effect : AbstractCollidableObject
{
	[SerializeField]
	protected bool randomRotation;

	[Space(10f)]
	[SerializeField]
	protected bool randomMirrorX;

	[SerializeField]
	protected bool randomMirrorY;

	public bool inUse;

	public bool removeOnEnd;

	public virtual Effect Create(Vector3 position)
	{
		return Create(position, Vector3.one);
	}

	public virtual Effect Create(Vector3 position, Vector3 scale)
	{
		Effect component = UnityEngine.Object.Instantiate(base.gameObject).GetComponent<Effect>();
		component.name = component.name.Replace("(Clone)", string.Empty);
		if (randomMirrorX)
		{
			scale.x = ((!Rand.Bool()) ? (0f - scale.x) : scale.x);
		}
		if (randomMirrorY)
		{
			scale.y = ((!Rand.Bool()) ? (0f - scale.y) : scale.y);
		}
		component.Initialize(position, scale, randomRotation);
		return component;
	}

	public virtual void Initialize(Vector3 position)
	{
		Vector3 scale = new Vector3(1f, 1f);
		if (randomMirrorX)
		{
			scale.x = ((!Rand.Bool()) ? (0f - scale.x) : scale.x);
		}
		if (randomMirrorY)
		{
			scale.y = ((!Rand.Bool()) ? (0f - scale.y) : scale.y);
		}
		Initialize(position, scale, randomRotation);
	}

	public virtual void Initialize(Vector3 position, Vector3 scale, bool randomR)
	{
		int value = UnityEngine.Random.Range(0, base.animator.GetInteger("Count"));
		base.animator.SetInteger("Effect", value);
		Transform transform = base.transform;
		transform.position = position;
		transform.localScale = scale;
		if (randomR)
		{
			transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, 360f));
		}
	}

	protected virtual void OnEffectCompletePool()
	{
		if (removeOnEnd)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			inUse = false;
		}
	}

	protected virtual void OnEffectComplete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Play()
	{
		base.animator.Play("A");
	}
}
