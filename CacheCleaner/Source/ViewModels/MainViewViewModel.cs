using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Threading;
using CacheCleaner.Models;
using ReactiveUI;

namespace CacheCleaner.ViewModels
{
	public class MainViewViewModel : ViewModelBase
	{
		public CleaningEntryModel[] CleaningEntryModelTest => new[]
		{
			new CleaningEntryModel() {Path = Environment.ExpandEnvironmentVariables("%temp%"), Files = 123, Folders = 456, Status = "Not Scanned"}
		};
		
		public CleaningEntry[] CleaningEntries => JsonSerializer.Deserialize<CleaningEntry[]>(File.ReadAllText("Resources/CleaningEntries.json"), new JsonSerializerOptions {PropertyNameCaseInsensitive = true})!;

		public ObservableCollection<CleaningEntryModel> Items { get; } = new ObservableCollection<CleaningEntryModel>();
		
		// Is not a list because it is updated across threads
		private readonly ConcurrentBag<CleaningEntryFileEnumeration> fileEnumerations = new ConcurrentBag<CleaningEntryFileEnumeration>();

		private readonly ConcurrentBag<CleaningEntryExecutionResult> executionResults =
			new ConcurrentBag<CleaningEntryExecutionResult>();

		private bool scanButtonEnabled = true;
		public bool ScanButtonEnabled
		{
			get => scanButtonEnabled;
			set => this.RaiseAndSetIfChanged(ref scanButtonEnabled, value);
		}

		private bool deleteButtonEnabled;

		public bool DeleteButtonEnabled
		{
			get => deleteButtonEnabled;
			set => this.RaiseAndSetIfChanged(ref deleteButtonEnabled, value);
		}
		
		public void Scan()
		{
			ScanButtonEnabled = false;
			DeleteButtonEnabled = false;
			List<Task> tasks = new List<Task>();
			Items.Clear();
			fileEnumerations.Clear();
			foreach (CleaningEntry cleaningEntry in CleaningEntries)
			{
				CleaningEntryModel entryModel = new CleaningEntryModel
				{
					Path = FileDeleter.ExpandNormalize(cleaningEntry.Directory),
					Status = "Scanning"
				};
				Items.Add(entryModel);


				tasks.Add(new Task(() =>
				{
					CleaningEntryFileEnumeration enumeration = FileDeleter.GetEntryFileEnumeration(cleaningEntry).Result;
					entryModel.Files = enumeration.Files.Count;
					entryModel.Folders = enumeration.Folders.Count;
					entryModel.Status = "Scanned";
					fileEnumerations.Add(enumeration);
				}));
			}

			new Task(() =>
			{
				Task.WaitAll(tasks.ToArray());
				ScanButtonEnabled = true;
				DeleteButtonEnabled = true;
			}).Start();
			
			tasks.ForEach(t => t.Start());
		}

		public void Delete()
		{
			DeleteButtonEnabled = false;
			List<Task> tasks = new List<Task>();
			foreach (CleaningEntryFileEnumeration entry in fileEnumerations)
			{
				CleaningEntryModel uiModelEntry = Items.First(t => t.Path == entry.Path);
				uiModelEntry.Status = "Deleting";
				tasks.Add(new Task(() =>
				{
					CleaningEntryExecutionResult entryExecutionResult = FileDeleter.ExecuteCleaningEntry(entry);

					uiModelEntry.Status =
						$"Deleted {entryExecutionResult.SuccessFiles.Count} files and {entryExecutionResult.SuccessFolders.Count} " +
						$"folders. Failed to delete {entryExecutionResult.FailureFiles.Count} files and " +
						$"{entryExecutionResult.FailureFolders.Count} folders";
					
					executionResults.Add(entryExecutionResult);
				}));
			}
			
			tasks.ForEach(t => t.Start());
			
		}
	}
}