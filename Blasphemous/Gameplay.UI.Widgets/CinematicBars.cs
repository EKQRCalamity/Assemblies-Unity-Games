using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.UI.Widgets;

[RequireComponent(typeof(Animator))]
public class CinematicBars : UIWidget
{
	private Animator animator;

	public bool InCinematicMode { get; private set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void CinematicMode(bool active)
	{
		Log.Trace("Cinematic", "Cinematic mode is " + active);
		InCinematicMode = active;
		animator.SetBool("SHOW", active);
		Core.UI.GameplayUI.gameObject.SetActive(!active);
		Core.Input.SetBlocker("CINEMATIC_MODE", active);
	}
}
