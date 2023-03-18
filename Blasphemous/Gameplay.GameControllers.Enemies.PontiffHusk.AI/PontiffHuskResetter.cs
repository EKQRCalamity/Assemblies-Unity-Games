using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.AI;

public class PontiffHuskResetter : MonoBehaviour
{
	private List<PontiffHuskRangedBehaviour> huskies = new List<PontiffHuskRangedBehaviour>();

	public IEnumerator Start()
	{
		yield return new WaitForSeconds(1f);
		huskies = new List<PontiffHuskRangedBehaviour>(Object.FindObjectsOfType<PontiffHuskRangedBehaviour>());
	}

	public void ResetState()
	{
		huskies.ForEach(delegate(PontiffHuskRangedBehaviour x)
		{
			x.ResetState();
		});
	}
}
