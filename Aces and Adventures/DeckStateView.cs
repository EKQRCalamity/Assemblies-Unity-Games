public class DeckStateView : AStateView
{
	public BoolEvent onBoxOpenChange;

	private bool _boxOpen;

	public bool boxOpen
	{
		get
		{
			return _boxOpen;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _boxOpen, value))
			{
				onBoxOpenChange?.Invoke(value);
			}
		}
	}
}
