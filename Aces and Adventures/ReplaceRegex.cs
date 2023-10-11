using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class ReplaceRegex
{
	private readonly Regex _regex;

	private readonly Dictionary<string, string> _remap;

	private readonly MatchEvaluator _matchEvaluator;

	public ReplaceRegex(Dictionary<string, string> remap, RegexOptions options = RegexOptions.None)
	{
		_remap = remap;
		_regex = new Regex(string.Join("|", _remap.Keys.Select(Regex.Escape)), options);
		_matchEvaluator = (Match m) => _remap[m.Value];
	}

	public string Replace(string s)
	{
		return _regex.Replace(s, _matchEvaluator);
	}
}
