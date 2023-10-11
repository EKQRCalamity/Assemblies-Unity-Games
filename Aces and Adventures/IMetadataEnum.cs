using System;
using UnityEngine.Localization.Metadata;

public interface IMetadataEnum<T> : IMetadata where T : struct, IConvertible
{
	T value { get; set; }
}
