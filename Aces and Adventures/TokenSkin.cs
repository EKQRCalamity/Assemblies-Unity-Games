using System;
using UnityEngine;

public class TokenSkin : MonoBehaviour
{
	[Serializable]
	public class Data
	{
		public Mesh mesh;

		public Material material;
	}

	public Data attack;

	public Data this[ChipType chip]
	{
		get
		{
			if (chip == ChipType.Attack)
			{
				return attack;
			}
			return attack;
		}
	}
}
