using System;
using System.Collections;
using UnityEngine;

public class CircusPlatformingLevelClouds : AbstractPausableComponent
{
	[Serializable]
	public class CloudPiece
	{
		public Transform cloud;

		public float cloudEndY;

		public float cameraRelativePosX;

		public float speedMultiplyAmount;

		private float currentRelativePosX;

		public void UpdateCurrentRelativePos(float pos)
		{
			currentRelativePosX = pos;
		}

		public float CurrentRelativePosX()
		{
			return currentRelativePosX;
		}
	}

	[SerializeField]
	private CloudPiece[] cloudPieces;

	private Vector3 lastPosition;

	[SerializeField]
	private float incrementAmount = 2f;

	private void Start()
	{
		StartCoroutine(change_y_axis());
	}

	private IEnumerator change_y_axis()
	{
		float[] cloudStartPositionsX = new float[cloudPieces.Length];
		float[] cloudStartSpeedX = new float[cloudPieces.Length];
		for (int j = 0; j < cloudPieces.Length; j++)
		{
			cloudStartPositionsX[j] = cloudPieces[j].cloud.position.y;
			ScrollingSprite[] componentsInChildren = cloudPieces[j].cloud.GetComponentsInChildren<ScrollingSprite>();
			foreach (ScrollingSprite scrollingSprite in componentsInChildren)
			{
				cloudStartSpeedX[j] = scrollingSprite.speed;
			}
		}
		while (true)
		{
			for (int i = 0; i < cloudPieces.Length; i++)
			{
				cloudPieces[i].cloud.SetPosition(null, Mathf.Lerp(cloudStartPositionsX[i], cloudPieces[i].cloudEndY, RelativePosition(cloudPieces[i].cameraRelativePosX)));
				if (CupheadLevelCamera.Current.transform.position != lastPosition)
				{
					if ((bool)cloudPieces[i].cloud.GetComponent<PlatformingLevelParallax>())
					{
						cloudPieces[i].cloud.GetComponent<PlatformingLevelParallax>().enabled = false;
					}
					ScrollingSprite[] componentsInChildren2 = cloudPieces[i].cloud.GetComponentsInChildren<ScrollingSprite>();
					foreach (ScrollingSprite scrollingSprite2 in componentsInChildren2)
					{
						if (CupheadLevelCamera.Current.transform.position.x < lastPosition.x)
						{
							if (scrollingSprite2.speed > cloudStartSpeedX[i])
							{
								scrollingSprite2.speed -= incrementAmount;
							}
						}
						else if (scrollingSprite2.speed < cloudStartSpeedX[i] * cloudPieces[i].speedMultiplyAmount)
						{
							scrollingSprite2.speed += incrementAmount;
						}
					}
					cloudPieces[i].UpdateCurrentRelativePos(RelativePosition(cloudPieces[i].cameraRelativePosX));
					lastPosition = CupheadLevelCamera.Current.transform.position;
					yield return null;
					continue;
				}
				if ((bool)cloudPieces[i].cloud.GetComponent<PlatformingLevelParallax>())
				{
					cloudPieces[i].cloud.GetComponent<PlatformingLevelParallax>().enabled = true;
					cloudPieces[i].cloud.GetComponent<PlatformingLevelParallax>().UpdateBasePosition();
				}
				ScrollingSprite[] componentsInChildren3 = cloudPieces[i].cloud.GetComponentsInChildren<ScrollingSprite>();
				foreach (ScrollingSprite scrollingSprite3 in componentsInChildren3)
				{
					if (scrollingSprite3.speed > cloudStartSpeedX[i])
					{
						scrollingSprite3.speed -= incrementAmount;
					}
					else
					{
						scrollingSprite3.speed = cloudStartSpeedX[i];
					}
				}
				yield return null;
			}
			yield return null;
		}
	}

	private float RelativePosition(float relativePosX)
	{
		float num = relativePosX - (float)Level.Current.Left;
		float num2 = CupheadLevelCamera.Current.transform.position.x - (float)Level.Current.Left;
		return num2 / num;
	}
}
