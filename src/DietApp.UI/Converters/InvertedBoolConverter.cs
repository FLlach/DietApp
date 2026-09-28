using System.Globalization;

namespace DietApp.UI.Converters;

/// <summary>
/// Como funciona: Convertidor de valor booleano que invierte el valor recibido (True -> False, False -> True).
/// Permite enlazar visibilidad inversa sin duplicar propiedades booleanas en los ViewModels.
/// Por que se tomo esta decision: Evita logica redundante en la capa de presentacion y permite
/// controlar la visibilidad de vistas condicionales directamente desde XAML.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return false;
    }
}
