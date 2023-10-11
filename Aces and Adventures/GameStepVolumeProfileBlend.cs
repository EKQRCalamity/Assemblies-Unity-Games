using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameStepVolumeProfileBlend : GameStep
{
	private Volume _start;

	private Volume _end;

	private float _blendTime;

	private float _elapsedTime;

	public GameStepVolumeProfileBlend(Volume start, Volume end, float blendTime = 0.25f)
	{
		_start = start;
		_end = end;
		_blendTime = Mathf.Max(0.001f, blendTime);
	}

	public override void Start()
	{
		Volume start = _start;
		Volume end = _end;
		Volume start2 = _start;
		bool flag2 = (_end.isGlobal = true);
		bool flag4 = (start2.isGlobal = flag2);
		bool flag6 = (end.enabled = flag4);
		start.enabled = flag6;
	}

	protected override IEnumerator Update()
	{
		while (true)
		{
			float num = Mathf.Clamp01(_elapsedTime / _blendTime);
			_end.weight = num;
			_start.weight = 1f - num;
			_elapsedTime += Time.deltaTime;
			if (num >= 1f)
			{
				break;
			}
			yield return null;
		}
	}

	protected override void End()
	{
		_start.enabled = false;
	}
}
