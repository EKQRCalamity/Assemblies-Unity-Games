using UnityEngine;

public static class KinematicUtilities
{
	public static float CalculateAcceleration(float distance, float finalSpeed)
	{
		return 0.5f * finalSpeed * finalSpeed / distance;
	}

	public static float CalculateTimeToSpeed(float distance, float finalSpeed)
	{
		return 2f * distance / finalSpeed;
	}

	public static float CalculateTimeToTravelDistance(float distance, float speed)
	{
		return distance / speed;
	}

	public static float CalculateVelocityFromZero(float distance, float time)
	{
		return 2f * distance / time;
	}

	public static float CalculateAccelerationFromZero(float distance, float time)
	{
		float num = time * time;
		return 2f * distance / num;
	}

	public static float CalculateTimeToChangeVelocity(float v1, float v2, float distance)
	{
		return 2f * distance / (v1 + v2);
	}

	public static float CalculateInitialSpeedToReachApex(float apexHeight, float gravity)
	{
		return Mathf.Sqrt(2f * gravity * apexHeight);
	}

	public static float CalculateDistanceTravelled(Vector2 initialVelocity, float startingHeight, float gravity)
	{
		return initialVelocity.x / gravity * (initialVelocity.y + Mathf.Sqrt(initialVelocity.y * initialVelocity.y + 2f * gravity * startingHeight));
	}

	public static float CalculateHorizontalSpeedToTravelDistance(float distance, float velocityY, float startingHeight, float gravity)
	{
		return distance * gravity / (velocityY + Mathf.Sqrt(velocityY * velocityY + 2f * gravity * startingHeight));
	}
}
