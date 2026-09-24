namespace DietApp.UI.Localization;

/// <summary>
/// Como funciona: Extension de marcado XAML que resuelve claves de texto localizadas y las enlaza
/// con LocalizationResourceManager en modo OneWay.
/// Por que se tomo esta decision: Permite una sintaxis limpia y declarativa en XAML ({loc:Translate Clave_Texto})
/// con soporte para reactividad inmediata ante cambios de idioma sin codigo repetitivo en el code-behind.
/// </summary>
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationResourceManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
