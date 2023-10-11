using System;
using System.Collections.Generic;
using UnityEngine.Localization;

[Serializable]
public abstract class GlossarySource
{
	public struct Term
	{
		public string source;

		public string target;

		public bool verbatim;

		public bool properNoun;

		public ProtectedTermWhen protectedTermWhen;

		public bool isProtected => protectedTermWhen switch
		{
			ProtectedTermWhen.Never => false, 
			ProtectedTermWhen.Verbatim => verbatim, 
			_ => true, 
		};

		public bool includeInGlossary => protectedTermWhen != ProtectedTermWhen.Exclusively;

		public Term(string source, string target, bool verbatim, bool properNoun, ProtectedTermWhen protectedTermWhen)
		{
			this.source = source;
			this.target = target;
			this.verbatim = verbatim;
			this.properNoun = properNoun;
			this.protectedTermWhen = protectedTermWhen;
		}
	}

	public abstract IAsyncEnumerable<Term> GetTermsAsync(Locale locale);
}
