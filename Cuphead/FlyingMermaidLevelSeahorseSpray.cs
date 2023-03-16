using System.Collections.Generic;
using UnityEngine;

public class FlyingMermaidLevelSeahorseSpray : AbstractPausableComponent
{
	private class PlayerInfo
	{
		public PlanePlayerMotor.Force force;

		public float timeSinceFx;

		public float fxWaitTime;

		public int lastFxVariant = -1;
	}

	public float width = 20f;

	private Dictionary<PlanePlayerMotor, PlayerInfo> playerInfos = new Dictionary<PlanePlayerMotor, PlayerInfo>();

	[SerializeField]
	private Effect effectPrefab;

	[SerializeField]
	private Transform topRoot;

	private bool ended;

	private void Update()
	{
		if (ended)
		{
			return;
		}
		foreach (PlanePlayerMotor key in playerInfos.Keys)
		{
			if (key == null)
			{
				continue;
			}
			if (Mathf.Abs(key.transform.position.x - base.transform.position.x) < width / 2f && key.player.center.y < topRoot.position.y)
			{
				playerInfos[key].force.enabled = true;
				playerInfos[key].timeSinceFx += CupheadTime.Delta;
				if (playerInfos[key].timeSinceFx >= playerInfos[key].fxWaitTime)
				{
					Effect effect = effectPrefab.Create(key.player.center + new Vector3(0f, -40f));
					int num = (playerInfos[key].lastFxVariant + Random.Range(0, 3)) % 3;
					effect.animator.SetInteger("Effect", num);
					playerInfos[key].lastFxVariant = num;
					playerInfos[key].fxWaitTime = Random.Range(0.125f, 0.17f);
					playerInfos[key].timeSinceFx = 0f;
				}
			}
			else
			{
				playerInfos[key].force.enabled = false;
				playerInfos[key].fxWaitTime = 0f;
			}
		}
	}

	public void Init(LevelProperties.FlyingMermaid.Seahorse properties)
	{
		foreach (PlanePlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				PlanePlayerMotor.Force force = new PlanePlayerMotor.Force(new Vector2(0f, properties.waterForce), enabled: false);
				allPlayer.motor.AddForce(force);
				PlayerInfo playerInfo = new PlayerInfo();
				playerInfo.force = force;
				playerInfos[allPlayer.motor] = playerInfo;
			}
		}
	}

	public void End()
	{
		ended = true;
		foreach (PlanePlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				allPlayer.motor.RemoveForce(playerInfos[allPlayer.motor].force);
			}
		}
	}
}
