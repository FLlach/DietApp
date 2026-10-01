using DietApp.Domain.Entities;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla relacional en SQLite para almacenar el perfil y las preferencias globales del usuario.
/// Mantiene la columna `UserId` con indice para busquedas instantaneas y preparacion para esquemas remotos (Supabase).
/// Por que se tomo esta decision: Separa el esquema de persistencia fisica del modelo de dominio,
/// permitiendo anadir atributos de serializacion y mapeo especificos de SQLite sin contaminar la entidad de negocio.
/// </summary>
[Table("UserProfiles")]
public class UserProfileEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string UserId { get; set; } = "local_user";

    public string DisplayName { get; set; } = "Usuario Principal";

    public string? Email { get; set; }

    public string DietaryCondition { get; set; } = "Renal / KDOQI";

    public double? WeightKg { get; set; }

    public double? HeightCm { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? BiologicalSex { get; set; }

    public double DailyCalorieTarget { get; set; } = 2000.0;

    public bool IsCalorieGoalEnabled { get; set; } = true;

    public double DailyProteinTargetGrams { get; set; } = 60.0;

    public bool IsProteinGoalEnabled { get; set; } = true;

    public double WarningThresholdPercentage { get; set; } = 80.0;

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

    public string PreferredLanguage { get; set; } = "es";

    public string ThemePreference { get; set; } = "Dark";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserProfile ToDomain()
    {
        return new UserProfile(
            id: Id,
            userId: UserId,
            displayName: DisplayName,
            email: Email,
            dietaryCondition: DietaryCondition,
            weightKg: WeightKg,
            heightCm: HeightCm,
            birthDate: BirthDate,
            biologicalSex: BiologicalSex,
            dailyCalorieTarget: DailyCalorieTarget,
            isCalorieGoalEnabled: IsCalorieGoalEnabled,
            dailyProteinTargetGrams: DailyProteinTargetGrams,
            isProteinGoalEnabled: IsProteinGoalEnabled,
            warningThresholdPercentage: WarningThresholdPercentage,
            potassiumLimitMg: PotassiumLimitMg,
            isPotassiumEnabled: IsPotassiumEnabled,
            phosphorusLimitMg: PhosphorusLimitMg,
            isPhosphorusEnabled: IsPhosphorusEnabled,
            sodiumLimitMg: SodiumLimitMg,
            isSodiumEnabled: IsSodiumEnabled,
            calciumLimitMg: CalciumLimitMg,
            isCalciumEnabled: IsCalciumEnabled,
            magnesiumLimitMg: MagnesiumLimitMg,
            isMagnesiumEnabled: IsMagnesiumEnabled,
            ironLimitMg: IronLimitMg,
            isIronEnabled: IsIronEnabled,
            zincLimitMg: ZincLimitMg,
            isZincEnabled: IsZincEnabled,
            preferredLanguage: PreferredLanguage,
            themePreference: ThemePreference,
            createdAt: CreatedAt,
            updatedAt: UpdatedAt);
    }

    public static UserProfileEntity FromDomain(UserProfile domain)
    {
        return new UserProfileEntity
        {
            Id = domain.Id,
            UserId = domain.UserId,
            DisplayName = domain.DisplayName,
            Email = domain.Email,
            DietaryCondition = domain.DietaryCondition,
            WeightKg = domain.WeightKg,
            HeightCm = domain.HeightCm,
            BirthDate = domain.BirthDate,
            BiologicalSex = domain.BiologicalSex,
            DailyCalorieTarget = domain.DailyCalorieTarget,
            IsCalorieGoalEnabled = domain.IsCalorieGoalEnabled,
            DailyProteinTargetGrams = domain.DailyProteinTargetGrams,
            IsProteinGoalEnabled = domain.IsProteinGoalEnabled,
            WarningThresholdPercentage = domain.WarningThresholdPercentage,
            PotassiumLimitMg = domain.PotassiumLimitMg,
            IsPotassiumEnabled = domain.IsPotassiumEnabled,
            PhosphorusLimitMg = domain.PhosphorusLimitMg,
            IsPhosphorusEnabled = domain.IsPhosphorusEnabled,
            SodiumLimitMg = domain.SodiumLimitMg,
            IsSodiumEnabled = domain.IsSodiumEnabled,
            CalciumLimitMg = domain.CalciumLimitMg,
            IsCalciumEnabled = domain.IsCalciumEnabled,
            MagnesiumLimitMg = domain.MagnesiumLimitMg,
            IsMagnesiumEnabled = domain.IsMagnesiumEnabled,
            IronLimitMg = domain.IronLimitMg,
            IsIronEnabled = domain.IsIronEnabled,
            ZincLimitMg = domain.ZincLimitMg,
            IsZincEnabled = domain.IsZincEnabled,
            PreferredLanguage = domain.PreferredLanguage,
            ThemePreference = domain.ThemePreference,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
}
