using UnityEngine;

public class AudioListenerHook : MonoBehaviour
{
	public void SetAudioListenerEnabled(bool setEnabled)
	{
		if (setEnabled)
		{
			MasterMixManager.Instance.controller.masterVolume.RemoveMultiplier(this);
		}
		else
		{
			MasterMixManager.Instance.controller.masterVolume.SetMultiplier(this, 0f);
		}
	}
}
