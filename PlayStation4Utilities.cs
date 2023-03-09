using System;
using System.Runtime.InteropServices;

public static class PlayStation4Utilities
{
	public static readonly int SCE_OK;

	public static readonly int SCE_SYSTEM_SERVICE_PARAM_ID_ENTER_BUTTON_ASSIGN = 1000;

	public static readonly int SCE_SYSTEM_PARAM_ENTER_BUTTON_ASSIGN_CIRCLE;

	public static readonly int SCE_SYSTEM_PARAM_ENTER_BUTTON_ASSIGN_CROSS = 1;

	[DllImport("GetParam")]
	private static extern int get_system_service_param(int param, out int value);

	public static int GetSystemServiceParam(int param)
	{
		int value;
		int num = get_system_service_param(param, out value);
		if (num != SCE_OK)
		{
			throw new Exception("Error getting param. Result code: " + num);
		}
		return value;
	}
}
