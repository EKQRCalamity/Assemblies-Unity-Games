public class MapCompassDeckLayout : ADeckLayout<MapCompass.Pile, MapCompass>
{
	public ACardLayout active;

	public ACardLayout inactive;

	public ACardLayout activeFloating;

	protected override ACardLayout this[MapCompass.Pile? pile]
	{
		get
		{
			return pile switch
			{
				MapCompass.Pile.Active => active, 
				MapCompass.Pile.Inactive => inactive, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case MapCompass.Pile.Active:
					active = value;
					break;
				case MapCompass.Pile.Inactive:
					inactive = value;
					break;
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(MapCompass value)
	{
		return MapCompassView.Create(value);
	}
}
