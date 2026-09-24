using System.ComponentModel;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato del servicio de localizacion que gestiona el idioma activo de la aplicacion,
/// provee cadenas traducidas por clave y notifica a los suscriptores cuando ocurre un cambio de idioma.
/// Por que se tomo esta decision: Centraliza la internacionalizacion en la capa de aplicacion para que
/// tanto la interfaz grafica como los mapeadores DTO y servicios de exportacion accedan a traducciones
/// coherentes sin acoplarse a recursos estaticos dependientes de plataforma.
/// </summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLanguage { get; }
    void SetLanguage(string languageCode);
    string GetString(string key);
    string this[string key] { get; }
    event EventHandler? LanguageChanged;
    IReadOnlyList<string> GetSupportedLanguages();
    string GetMineralName(MineralType mineralType);
    string GetMealTypeName(MealType mealType);
}
