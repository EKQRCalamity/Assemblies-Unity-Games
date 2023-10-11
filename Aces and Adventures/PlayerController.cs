using Dynamite3D.RealIvy;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public IvyCaster ivyCaster;

	public Transform trIvy;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			ivyCaster.CastRandomIvy(trIvy.position, trIvy.rotation);
		}
	}
}
