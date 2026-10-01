using DietApp.Domain.Entities;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de datos para mapeo de la tabla `user_profiles` en PostgreSQL a traves del cliente PostgREST de Supabase.
/// Hereda de `BaseModel` y utiliza los atributos de mapeo de columnas snake_case a propiedades en PascalCase.
/// Por que se tomo esta decision: Permite serializar y deserializar automaticamente las respuestas HTTP
/// de la API REST de Supabase hacia el modelo de objetos C# y viceversa.
/// </summary>
[Table("user_profiles")]
public class SupabaseUserProfileModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("dietary_condition")]
    public string DietaryCondition { get; set; } = "Renal / KDOQI";

    [Column("weight_kg")]
    public double? WeightKg { get; set; }

    [Column("height_cm")]
    public double? HeightCm { get; set; }

    [Column("birth_date")]
    public DateTime? BirthDate { get; set; }

    [Column("biological_sex")]
    public string? BiologicalSex { get; set; }

    [Column("daily_calorie_target")]
    public double DailyCalorieTarget { get; set; } = 2000.0;

    [Column("is_calorie_goal_enabled")]
    public bool IsCalorieGoalEnabled { get; set; } = true;

    [Column("daily_protein_target_grams")]
    public double DailyProteinTargetGrams { get; set; } = 60.0;

    [Column("is_protein_goal_enabled")]
    public bool IsProteinGoalEnabled { get; set; } = true;

    [Column("warning_threshold_percentage")]
    public double WarningThresholdPercentage { get; set; } = 80.0;

    [Column("potassium_limit_mg")]
    public double PotassiumLimitMg { get; set; } = 2000.0;

    [Column("is_potassium_enabled")]
    public bool IsPotassiumEnabled { get; set; } = true;

    [Column("phosphorus_limit_mg")]
    public double PhosphorusLimitMg { get; set; } = 850.0;

    [Column("is_phosphorus_enabled")]
    public bool IsPhosphorusEnabled { get; set; } = true;

    [Column("sodium_limit_mg")]
    public double SodiumLimitMg { get; set; } = 1500.0;

    [Column("is_sodium_enabled")]
    public bool IsSodiumEnabled { get; set; } = true;

    [Column("calcium_limit_mg")]
    public double CalciumLimitMg { get; set; } = 1000.0;

    [Column("is_calcium_enabled")]
    public bool IsCalciumEnabled { get; set; } = true;

    [Column("magnesium_limit_mg")]
    public double MagnesiumLimitMg { get; set; } = 350.0;

    [Column("is_magnesium_enabled")]
    public bool IsMagnesiumEnabled { get; set; } = true;

    [Column("iron_limit_mg")]
    public double IronLimitMg { get; set; } = 15.0;

    [Column("is_iron_enabled")]
    public bool IsIronEnabled { get; set; } = true;

    [Column("zinc_limit_mg")]
    public double ZincLimitMg { get; set; } = 12.0;

    [Column("is_zinc_enabled")]
    public bool IsZincEnabled { get; set; } = true;

    [Column("preferred_language")]
    public string PreferredLanguage { get; set; } = "es";

    [Column("theme_preference")]
    public string ThemePreference { get; set; } = "Dark";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public UserProfile ToDomain()
    {
        Guid.TryParse(Id, out var parsedId);

        return new UserProfile(
            id: parsedId == Guid.Empty ? Guid.NewGuid() : parsedId,
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

    public static SupabaseUserProfileModel FromDomain(UserProfile domain)
    {
        return new SupabaseUserProfileModel
        {
            Id = domain.Id.ToString(),
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
