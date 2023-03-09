using UnityEngine;

public class HarbourPlatformingLevelOctopusHead : LevelPlatform
{
	[SerializeField]
	private HarbourPlatformingLevelOctopus octopus;

	public override void AddChild(Transform player)
	{
		base.AddChild(player);
		octopus.animator.SetBool("playerOn", value: true);
	}

	public override void OnPlayerExit(Transform player)
	{
		base.OnPlayerExit(player);
		if (base.transform.childCount <= 1)
		{
			octopus.animator.SetBool("playerOn", value: false);
		}
	}
}
