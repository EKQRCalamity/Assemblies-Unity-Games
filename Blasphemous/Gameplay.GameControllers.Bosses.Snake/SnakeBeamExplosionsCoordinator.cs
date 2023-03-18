using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeBeamExplosionsCoordinator : MonoBehaviour
{
	public List<Animator> Animators;

	public void PlayBeams()
	{
		Animators.ForEach(delegate(Animator x)
		{
			x.SetBool("BEAM", value: true);
		});
	}

	public void StopBeams()
	{
		Animators.ForEach(delegate(Animator x)
		{
			x.SetBool("BEAM", value: false);
		});
	}

	public void PlayWarning()
	{
		Animators.ForEach(delegate(Animator x)
		{
			x.SetTrigger("WARNING");
		});
	}
}
