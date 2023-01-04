using System;

namespace CacheCleaner;

public class DeletionFailureCause
{
	public string Path { get; init; }
	public Exception Cause { get; init; }
}