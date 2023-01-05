using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CacheCleaner.Models;

namespace CacheCleaner;

public class RowConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is CleaningEntryModel { Status: "Scanned" })
		{
			return Brushes.Wheat;
		}
		
		return Brushes.Aquamarine;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return null;
	}
}