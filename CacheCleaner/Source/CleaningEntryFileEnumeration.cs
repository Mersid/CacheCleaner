using System.Collections.Generic;

namespace CacheCleaner;

public class CleaningEntryFileEnumeration
{
	public string Path { get; init; }
	public List<string> Files { get; init; }
	public List<string> Folders { get; init; }
}