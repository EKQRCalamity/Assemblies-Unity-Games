public static class CanvusInputFocusExtensions
{
	public static bool HasFocus(this CanvasInputFocus inputFocus)
	{
		if ((bool)inputFocus)
		{
			return inputFocus.hasFocus;
		}
		return false;
	}

	public static bool HasFocusPermissive(this CanvasInputFocus inputFocus)
	{
		if (!inputFocus.HasFocus())
		{
			return !CanvasInputFocus.HasActiveComponents;
		}
		return true;
	}
}
