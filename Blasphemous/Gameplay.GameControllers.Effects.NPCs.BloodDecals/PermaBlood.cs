using UnityEngine;

namespace Gameplay.GameControllers.Effects.NPCs.BloodDecals;

public class PermaBlood : MonoBehaviour
{
	public enum PermaBloodType
	{
		permablood_1,
		permablood_2
	}

	public PermaBloodType permaBloodType;

	private void Start()
	{
	}

	public GameObject Instance(Vector3 pos, Quaternion rotation)
	{
		return Object.Instantiate(base.transform.gameObject, pos, rotation);
	}
}
