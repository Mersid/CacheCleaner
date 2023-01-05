using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CacheCleaner.Models;

namespace CacheCleaner.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
	{
		// DataGridRow row = e.Row;
		// row.Bind(BackgroundProperty, new Binding("Brush", BindingMode.OneWay)
		// {
		// 	
		// });
	}
}