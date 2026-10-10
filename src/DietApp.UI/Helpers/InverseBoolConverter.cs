using System.Globalization;

namespace DietApp.UI.Helpers;

/// <summary>
/// Como funciona: Convertidor de valor que invierte valores booleanos (true -> false, false -> true).
/// Implementa IMarkupExtension para permitir su invocacion directa y concisa en marcado XAML.
/// Por que se tomo esta decision: Permite controlar propiedades dependientes como IsEnabled a partir
/// de estados de carga (IsBusy) directamente desde el marcado XAML sin duplicar propiedades en el ViewModel.
/// </summary>
public class InverseBoolConverter : IValueConverter, IMarkupExtension<InverseBoolConverter>
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return true;
    }

    public InverseBoolConverter ProvideValue(IServiceProvider serviceProvider) => this;

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => this;
}
