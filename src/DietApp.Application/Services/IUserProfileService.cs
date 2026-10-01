using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para la gestion integral del perfil del usuario, sus datos clinicos y preferencias.
/// Provee metodos para consultar el perfil activo, actualizar metas nutricionales (calorias y proteinas),
/// calibrar el umbral preventivo de advertencia, definir limites de minerales diana y fijar preferencias de interfaz.
/// Notifica reactivamente mediante el evento `ProfileChanged` ante cualquier actualizacion.
/// Por que se tomo esta decision: Centraliza en la capa de aplicacion la orquestacion del perfil y sus preferencias,
/// asegurando coherencia con los demas servicios del sistema y preparando una transicion limpia hacia Supabase.
/// </summary>
public interface IUserProfileService
{
    Task<UserProfileDto> GetCurrentProfileAsync();
    Task<UserProfileDto> SaveProfileAsync(UserProfileDto profileDto);
    Task UpdateProfileDetailsAsync(
        string displayName,
        string? email,
        string? dietaryCondition,
        double? weightKg,
        double? heightCm,
        DateTime? birthDate,
        string? biologicalSex);
    Task UpdateNutritionalGoalsAsync(
        double calorieTarget,
        bool isCalorieEnabled,
        double proteinTargetGrams,
        bool isProteinEnabled);
    Task UpdateWarningThresholdAsync(double thresholdPercent);
    Task UpdateMineralLimitAsync(MineralType mineral, double limitMg, bool isEnabled);
    Task UpdatePreferencesAsync(string preferredLanguage, string themePreference);
    Task ResetToDefaultsAsync();

    event EventHandler<UserProfileDto>? ProfileChanged;
}
