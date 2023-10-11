using UnityEngine;

public class GameObjectLayout : UILayout3D<GameObject>
{
	protected override bool _IsValid(GameObject data)
	{
		return data;
	}

	public override GameObject GenerateViewFromData(GameObject data)
	{
		return Object.Instantiate(data);
	}

	public override GameObject GetDataFromGameObject(GameObject go)
	{
		return go;
	}
}
