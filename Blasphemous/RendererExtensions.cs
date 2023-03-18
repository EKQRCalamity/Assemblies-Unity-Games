using UnityEngine;

public static class RendererExtensions
{
	private static readonly Plane[] planeBuffer = new Plane[6];

	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		GeometryUtility.CalculateFrustumPlanes(camera, planeBuffer);
		return GeometryUtility.TestPlanesAABB(planeBuffer, renderer.bounds);
	}
}
