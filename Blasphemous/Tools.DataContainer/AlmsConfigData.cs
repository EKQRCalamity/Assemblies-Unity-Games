using System.Collections.Generic;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.DataContainer;

[CreateAssetMenu(fileName = "AlmsConfig", menuName = "Blasphemous/Alms")]
public class AlmsConfigData : SerializedScriptableObject
{
	[BoxGroup("PrieDieus", true, false, 0)]
	public int Level2PrieDieus = 3;

	[BoxGroup("PrieDieus", true, false, 0)]
	public int Level3PrieDieus = 5;

	[BoxGroup("Widget Sounds", true, false, 0)]
	[InfoBox("Sound when alms added ok but no new tier reached", InfoMessageType.Info, null)]
	[EventRef]
	public string SoundAddedOk;

	[BoxGroup("Widget Sounds", true, false, 0)]
	[InfoBox("Sound when change value, the timing is determned by SoundChangeUpdate", InfoMessageType.Info, null)]
	[EventRef]
	public string SoundChange;

	[BoxGroup("Widget Sounds", true, false, 0)]
	[InfoBox("Time to wait curve to play sound change sound", InfoMessageType.Info, null)]
	public AnimationCurve SoundChangeUpdate;

	[BoxGroup("Widget Sounds", true, false, 0)]
	[InfoBox("Sound when alms added ok and new tier reached", InfoMessageType.Info, null)]
	[EventRef]
	public string SoundNewTier;

	[BoxGroup("Widget", true, false, 0)]
	[InfoBox("Time to wait until widget is showed", InfoMessageType.Info, null)]
	public float InitialDelay = 1f;

	[BoxGroup("Widget", true, false, 0)]
	[InfoBox("Speed in numbers per second with factor 1 in NumberFactorByTime", InfoMessageType.Info, null)]
	public float NumberSpeed;

	[BoxGroup("Widget", true, false, 0)]
	[InfoBox("Max number of tiers that can give in one action", InfoMessageType.Info, null)]
	public int MaxNumber;

	[InfoBox("Factor curve to multiply value depending on time pressing control to acelerate", InfoMessageType.Info, null)]
	[BoxGroup("Widget", true, false, 0)]
	public AnimationCurve NumberFactorByTime;

	[BoxGroup("Tiers", true, false, 0)]
	[ListDrawerSettings(IsReadOnly = true)]
	public int[] TearsToUnlock = new int[7] { 100, 200, 300, 400, 500, 600, 700 };

	public List<int> GetTearsList()
	{
		List<int> list = new List<int>(TearsToUnlock);
		list.Sort();
		return list;
	}
}
