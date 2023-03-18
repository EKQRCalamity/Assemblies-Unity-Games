using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BsDivineBeamManager : SerializedMonoBehaviour
{
	private const int BeamsPatternRows = 4;

	private const int BeamsPatternColumns = 19;

	private readonly WaitForSeconds _beamDelay = new WaitForSeconds(0.2f);

	private readonly List<Vector2> _divineBeamPositions = new List<Vector2>();

	private Vector2 _originPosition;

	[BoxGroup("Divine Beam Patterns", true, false, 0)]
	[TableMatrix(DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false)]
	public bool[,] CustomCellDrawing = new bool[19, 4];

	public GameObject DivineBeam;

	private readonly System.Random rnd = new System.Random();

	public DivineBeamOrigin DivineBeamOrigin { get; private set; }

	private void Start()
	{
		DivineBeamOrigin = GetComponentInChildren<DivineBeamOrigin>();
		PoolManager.Instance.CreatePool(DivineBeam, 10);
	}

	private void Shuffle<T>(IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = rnd.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public void InstantiateDivineBeams()
	{
		GenerateDivineBeamPositions();
		Shuffle(_divineBeamPositions);
		StartCoroutine(InstantiateBeamsCoroutine());
	}

	private IEnumerator InstantiateBeamsCoroutine()
	{
		foreach (Vector2 position in _divineBeamPositions)
		{
			PoolManager.Instance.ReuseObject(DivineBeam, position, Quaternion.identity);
			yield return _beamDelay;
		}
	}

	public void InstantiateSingleBeam(Vector2 pos)
	{
		pos.y = DivineBeamOrigin.OriginPosition.y;
		PoolManager.Instance.ReuseObject(DivineBeam, pos, Quaternion.identity);
	}

	private void GenerateDivineBeamPositions()
	{
		_originPosition = DivineBeamOrigin.OriginPosition;
		_divineBeamPositions.Clear();
		int num = UnityEngine.Random.Range(0, 4);
		float num2 = _originPosition.x;
		for (int i = 0; i < 19; i++)
		{
			if (CustomCellDrawing[i, num])
			{
				Vector2 item = new Vector2(num2, _originPosition.y);
				_divineBeamPositions.Add(item);
				num2 += 2f;
			}
			else
			{
				num2 += 1f;
			}
		}
	}
}
