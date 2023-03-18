using Framework.Managers;
using Gameplay.GameControllers.Environment.AreaEffects;
using UnityEngine;

public class VenomFog : MonoBehaviour
{
	public Color normalColor;

	public Color venomColor;

	public SpriteRenderer spr;

	public float maxReactDistance;

	public PoisonAreaEffect poisonArea;

	private void Awake()
	{
		spr = GetComponent<SpriteRenderer>();
		poisonArea = GetComponentInParent<PoisonAreaEffect>();
	}

	private void Update()
	{
		if (!(Core.Logic.Penitent == null) && poisonArea != null && poisonArea.IsDisabled)
		{
			float num = Vector2.Distance(Core.Logic.Penitent.transform.position, base.transform.position);
			if (num < maxReactDistance)
			{
				Color color = Color.Lerp(venomColor, normalColor, 1f - num / maxReactDistance);
				spr.color = color;
			}
		}
	}
}
