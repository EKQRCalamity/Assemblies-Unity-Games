using FMODUnity;
using Framework.Inventory;
using Framework.Managers;
using UnityEngine;

namespace Tools.Items;

public class ItemAudio : ObjectEffect
{
	[SerializeField]
	[EventRef]
	private string audioId;

	[SerializeField]
	private float delay;

	protected override bool OnApplyEffect()
	{
		Core.Audio.PlaySfx(audioId, delay);
		return true;
	}
}
