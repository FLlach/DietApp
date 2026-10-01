namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos integrales del perfil de usuario y sus preferencias
/// entre la logica de aplicacion, el almacenamiento de infraestructura y los ViewModels de la interfaz.
/// Por que se tomo esta decision: Desacopla las entidades del dominio de la capa de presentacion,
/// facilitando el enlace de datos y la preparacion para serializacion con servicios externos como Supabase.
/// </summary>
public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "local_user";
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DietaryCondition { get; set; } = "Renal / KDOQI";

    // Informacion Antropometrica
    public double? WeightKg { get; set; }
    public double? HeightCm { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? BiologicalSex { get; set; }

    // Metas Energeticas y Proteicas
    public double DailyCalorieTarget { get; set; } = 2000.0;
    public bool IsCalorieGoalEnabled { get; set; } = true;
    public double DailyProteinTargetGrams { get; set; } = 60.0;
    public bool IsProteinGoalEnabled { get; set; } = true;

    // Umbral Preventivo de Alerta
    public double WarningThresholdPercentage { get; set; } = 80.0;

    // Limites de Minerales Diana
    public double PotassiumLimitMg { get; set; } = 2000.0;
    public bool IsPotassiumEnabled { get; set; } = true;

    public double PhosphorusLimitMg { get; set; } = 850.0;
    public bool IsPhosphorusEnabled { get; set; } = true;

    public double SodiumLimitMg { get; set; } = 1500.0;
    public bool IsSodiumEnabled { get; set; } = true;

    public double CalciumLimitMg { get; set; } = 1000.0;
    public bool IsCalciumEnabled { get; set; } = true;

    public double MagnesiumLimitMg { get; set; } = 350.0;
    public bool IsMagnesiumEnabled { get; set; } = true;

    public double IronLimitMg { get; set; } = 15.0;
    public bool IsIronEnabled { get; set; } = true;

    public double ZincLimitMg { get; set; } = 12.0;
    public bool IsZincEnabled { get; set; } = true;

    // Preferencias de UI
    public string PreferredLanguage { get; set; } = "es";
    public string ThemePreference { get; set; } = "Dark";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string ClinicalSummary => $"{DisplayName} ({DietaryCondition}) - Meta: {DailyCalorieTarget:F0} kcal, {DailyProteinTargetGrams:F0}g prot.";
}
