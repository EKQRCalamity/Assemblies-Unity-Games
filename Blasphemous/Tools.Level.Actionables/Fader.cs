using DG.Tweening;
using UnityEngine;

namespace Tools.Level.Actionables;

[RequireComponent(typeof(SpriteRenderer))]
public class Fader : MonoBehaviour, IActionable
{
	public float time;

	public bool Locked { get; set; }

	public void Use()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		component.DOFade(0f, time);
	}
}
