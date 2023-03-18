using System.Timers;

namespace Framework.FrameworkCore.Attributes.Logic;

public class FinalBonus : BaseAttribute
{
	private Timer _timer;

	private Attribute _parentAttribute;

	public FinalBonus(float time, float baseValue, float baseMultiplier = 1f)
		: base(baseValue, baseMultiplier)
	{
		_timer = new Timer(time);
		_timer.Elapsed += TimerOnElapsed;
	}

	public void StartTimer(Attribute parentAttribute)
	{
		_parentAttribute = parentAttribute;
		_timer.Start();
	}

	private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
	{
		_timer.Stop();
		_parentAttribute.RemoveFinalBonus(this);
	}
}
