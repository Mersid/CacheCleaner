using System.Collections.Generic;

namespace CacheCleaner;

public class CleaningEntryExecutionResult
{
	public List<string> SuccessFiles { get; init; }
	public List<string> SuccessFolders { get; init; }
	public List<DeletionFailureCause> FailureFiles { get; init; }
	public List<DeletionFailureCause> FailureFolders { get; init; }
}