using UnityEngine;

public class RetroArcadeToadManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private RetroArcadeToad toadPrefab;

	private LevelProperties.RetroArcade.Toad p;

	private RetroArcadeToad toad1;

	private RetroArcadeToad toad2;

	private int numDied;

	public void StartToad()
	{
		p = base.properties.CurrentState.toad;
		numDied = 0;
		toad1 = toadPrefab.Create(this, p, onLeft: true);
		toad2 = toadPrefab.Create(this, p, onLeft: false);
	}

	public void OnToadDie()
	{
		numDied++;
		if (numDied >= 2)
		{
			StopAllCoroutines();
			Object.Destroy(toad1.gameObject);
			Object.Destroy(toad2.gameObject);
			base.properties.DealDamageToNextNamedState();
		}
	}
}
