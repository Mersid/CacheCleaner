using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CacheCleaner;

public class FileDeleter
{
	public static CleaningEntryExecutionResult ExecuteCleaningEntry(CleaningEntryFileEnumeration fileEnumeration)
	{
		List<string> successFiles = new List<string>();
		List<string> successFolders = new List<string>();
		List<DeletionFailureCause> failureFiles = new List<DeletionFailureCause>();
		List<DeletionFailureCause> failureFolders = new List<DeletionFailureCause>();
		
		// Delete all files first recursively
		foreach (string file in fileEnumeration.Files)
		{
			try
			{
				File.Delete(file);
				successFiles.Add(file);
			}
			catch (Exception e)
			{
				failureFiles.Add(new DeletionFailureCause {Path = file, Cause = e});
			}
		}
		
		// Now delete directories
		foreach (string directory in fileEnumeration.Folders)
		{
			try
			{
				Directory.Delete(directory);
				successFolders.Add(directory);
			}
			catch (Exception e)
			{
				failureFolders.Add(new DeletionFailureCause {Path = directory, Cause = e});
			}
		}

		CleaningEntryExecutionResult result = new CleaningEntryExecutionResult
			{ SuccessFiles = successFiles, SuccessFolders = successFolders, FailureFiles = failureFiles, FailureFolders = failureFolders};
		
		return result;
	}

	/// <summary>
	/// Gets a list of files in the cleaning entry, minus any excluded subdirectories
	/// </summary>
	public static async Task<CleaningEntryFileEnumeration> GetEntryFileEnumeration(CleaningEntry cleaningEntry)
	{
		// Gets a list of every file to exclude
		List<string> excludedFiles = new List<string>();
		foreach (string exclude in cleaningEntry.Excludes)
		{
			excludedFiles.AddRange(await GetFilesRecursively(ExpandNormalize(exclude)));
		}
		
		List<string> paths = (await GetFilesRecursively(ExpandNormalize(cleaningEntry.Directory))).Except(excludedFiles).ToList();
		List<string> files = paths.Where(File.Exists).ToList();
		List<string> folders = paths.Where(Directory.Exists).ToList();

		CleaningEntryFileEnumeration enumeration = new CleaningEntryFileEnumeration { Path = ExpandNormalize(cleaningEntry.Directory), Files = files, Folders = folders };
		return enumeration;
	}

	/// <summary>
	/// Gets a list of every file and folder in dir
	/// </summary>
	private static async Task<List<string>> GetFilesRecursively(string dir)
	{
		List<string> files = new List<string>();

		try
		{
			if (IsSymlink(dir))
				return files; // If it's a symlink, just return.
			
			files.AddRange(Directory.EnumerateFiles(dir));
			foreach (string subdir in Directory.EnumerateDirectories(dir))
			{
				files.Add(subdir);
				files.AddRange(await GetFilesRecursively(subdir));
			}
		}
		catch (Exception)
		{
			// If we fail to enumerate the current directory, just exit it without adding its children to the list
		}

		// Normalize file directories to Linux-style (personal preference, could be any other style)
		files = files.Select(t => t.Replace(Path.DirectorySeparatorChar, '/')).ToList();
		return files;
	}

	private static bool IsSymlink(string path)
	{
		// https://stackoverflow.com/questions/1485155/check-if-a-file-is-real-or-a-symbolic-link
		FileInfo pathInfo = new FileInfo(path);
		return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
	}

	/// <summary>
	/// Expands and normalizes path strings
	/// </summary>
	public static string ExpandNormalize(string path)
	{
		return Environment.ExpandEnvironmentVariables(path).Replace(Path.DirectorySeparatorChar, '/');
	}
}