public class PlmManager
{
	private static PlmManager instance;

	public static PlmManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PlmManager();
			}
			return instance;
		}
	}

	public PlmInterface Interface { get; private set; }

	public void Init()
	{
		Interface = new DummyPlmInterface();
		Interface.Init();
		Interface.OnSuspend += OnSuspend;
		Interface.OnResume += OnResume;
		Interface.OnConstrained += OnConstrained;
		Interface.OnUnconstrained += OnUnconstrained;
	}

	private void OnSuspend()
	{
	}

	private void OnResume()
	{
	}

	private void OnConstrained()
	{
	}

	private void OnUnconstrained()
	{
	}
}
