using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicePalaceDominoLevelFloor : AbstractCollidableObject
{
	[Header("Floor")]
	[SerializeField]
	private DicePalaceDominoLevelScrollingFloor[] _floors;

	[SerializeField]
	private ScrollingSprite _teethSprite;

	private DicePalaceDominoLevelBouncyBall.Colour spikesColour = DicePalaceDominoLevelBouncyBall.Colour.none;

	public Action OnToggleFlashEvent;

	public Action OnColourChangeEvent;

	private LevelProperties.DicePalaceDomino properties;

	private List<DicePalaceDominoLevelFloorTile> tiles;

	private List<DicePalaceDominoLevelFloorTile> preTiles;

	private LevelPlayerMotor.VelocityManager.Force levelForce;

	public void InitFloor(LevelProperties.DicePalaceDomino properties)
	{
		this.properties = properties;
		tiles = new List<DicePalaceDominoLevelFloorTile>();
		preTiles = new List<DicePalaceDominoLevelFloorTile>();
	}

	public void StartSpawningTiles()
	{
		StartCoroutine(tileSpawn_cr());
	}

	private IEnumerator tileSpawn_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 3f);
		for (int i = 0; i < _floors.Length; i++)
		{
			_floors[i].speed = properties.CurrentState.domino.floorSpeed;
		}
		_teethSprite.speed = properties.CurrentState.domino.floorSpeed;
		AddForces();
		for (int j = 0; j < preTiles.Count; j++)
		{
			if (preTiles[j].currentColourIndex == (int)spikesColour)
			{
				preTiles[j].TriggerSpikes(spikesActive: true);
			}
			else
			{
				preTiles[j].TriggerSpikes(spikesActive: false);
			}
			preTiles[j].InitTile();
		}
	}

	private void AddForces()
	{
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				levelForce = new LevelPlayerMotor.VelocityManager.Force(LevelPlayerMotor.VelocityManager.Force.Type.Ground, 0f - properties.CurrentState.domino.floorSpeed);
				allPlayer.motor.AddForce(levelForce);
			}
		}
	}

	private int ParseColour(char c)
	{
		return c switch
		{
			'B' => 0, 
			'Y' => 3, 
			'G' => 1, 
			'R' => 2, 
			_ => 0, 
		};
	}

	public void CheckTiles(DicePalaceDominoLevelBouncyBall.Colour color)
	{
		spikesColour = color;
		StartCoroutine(check_tiles_cr(spikesColour));
	}

	private IEnumerator check_tiles_cr(DicePalaceDominoLevelBouncyBall.Colour color)
	{
		foreach (DicePalaceDominoLevelFloorTile tile in tiles)
		{
			if (tile.isActivated)
			{
				if (tile.currentColourIndex == (int)color)
				{
					tile.TriggerSpikes(spikesActive: true);
				}
				else
				{
					tile.TriggerSpikes(spikesActive: false);
				}
			}
		}
		yield return null;
	}
}
