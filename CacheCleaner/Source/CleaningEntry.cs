using System.ComponentModel;
using JetBrains.Annotations;

namespace CacheCleaner;

[UsedImplicitly] // By JSON deserialization
public class CleaningEntry
{
	public string Directory { get; init; }
	public string[] Excludes { get; init; } = new string[] { };
}