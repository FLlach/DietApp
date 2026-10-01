using DietApp.Domain.Enums;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Raiz de Agregado (Aggregate Root) que encapsula el perfil del usuario,
/// sus datos antropometricos, metas nutricionales (calorias y proteinas), umbrales preventivos de alerta,
/// limites diarios cuantitativos de minerales diana (K, P, Na, Ca, Mg, Fe, Zn) y preferencias de interfaz (idioma y tema).
/// Mantiene la propiedad `UserId` como clave natural para asociacion con servicios de identidad y persistencia en la nube (como Supabase Auth).
/// Por que se tomo esta decision: Centraliza en el dominio el estado clinico y las preferencias del paciente,
/// garantizando reglas de validacion consistentes y preparando la futura migracion y sincronizacion con Supabase.
/// </summary>
public class UserProfile
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string DietaryCondition { get; private set; }

    // Datos Antropometricos y Clinicos
    public double? WeightKg { get; private set; }
    public double? HeightCm { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? BiologicalSex { get; private set; }

    // Metas Nutricionales Diarias
    public double DailyCalorieTarget { get; private set; }
    public bool IsCalorieGoalEnabled { get; private set; }
    public double DailyProteinTargetGrams { get; private set; }
    public bool IsProteinGoalEnabled { get; private set; }

    // Umbral Preventivo de Alerta Porcentual (50% a 95%)
    public double WarningThresholdPercentage { get; private set; }

    // Limites Diarios de los 7 Minerales Diana (en miligramos) y estado de activacion
    public double PotassiumLimitMg { get; private set; }
    public bool IsPotassiumEnabled { get; private set; }

    public double PhosphorusLimitMg { get; private set; }
    public bool IsPhosphorusEnabled { get; private set; }

    public double SodiumLimitMg { get; private set; }
    public bool IsSodiumEnabled { get; private set; }

    public double CalciumLimitMg { get; private set; }
    public bool IsCalciumEnabled { get; private set; }

    public double MagnesiumLimitMg { get; private set; }
    public bool IsMagnesiumEnabled { get; private set; }

    public double IronLimitMg { get; private set; }
    public bool IsIronEnabled { get; private set; }

    public double ZincLimitMg { get; private set; }
    public bool IsZincEnabled { get; private set; }

    // Preferencias de Interfaz
    public string PreferredLanguage { get; private set; }
    public string ThemePreference { get; private set; }

    // Metadatos de Auditoria
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public UserProfile(
        Guid id,
        string userId,
        string displayName,
        string? email,
        string dietaryCondition,
        double? weightKg,
        double? heightCm,
        DateTime? birthDate,
        string? biologicalSex,
        double dailyCalorieTarget,
        bool isCalorieGoalEnabled,
        double dailyProteinTargetGrams,
        bool isProteinGoalEnabled,
        double warningThresholdPercentage,
        double potassiumLimitMg,
        bool isPotassiumEnabled,
        double phosphorusLimitMg,
        bool isPhosphorusEnabled,
        double sodiumLimitMg,
        bool isSodiumEnabled,
        double calciumLimitMg,
        bool isCalciumEnabled,
        double magnesiumLimitMg,
        bool isMagnesiumEnabled,
        double ironLimitMg,
        bool isIronEnabled,
        double zincLimitMg,
        bool isZincEnabled,
        string preferredLanguage,
        string themePreference,
        DateTime createdAt,
        DateTime updatedAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = string.IsNullOrWhiteSpace(userId) ? "local_user" : userId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Usuario" : displayName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        DietaryCondition = string.IsNullOrWhiteSpace(dietaryCondition) ? "Renal / KDOQI" : dietaryCondition.Trim();

        WeightKg = weightKg is > 0 and < 500 ? weightKg : null;
        HeightCm = heightCm is > 0 and < 300 ? heightCm : null;
        BirthDate = birthDate;
        BiologicalSex = biologicalSex;

        DailyCalorieTarget = dailyCalorieTarget >= 0 ? dailyCalorieTarget : 2000.0;
        IsCalorieGoalEnabled = isCalorieGoalEnabled;
        DailyProteinTargetGrams = dailyProteinTargetGrams >= 0 ? dailyProteinTargetGrams : 60.0;
        IsProteinGoalEnabled = isProteinGoalEnabled;

        WarningThresholdPercentage = warningThresholdPercentage is >= 50.0 and <= 95.0
            ? warningThresholdPercentage
            : 80.0;

        PotassiumLimitMg = potassiumLimitMg > 0 ? potassiumLimitMg : 2000.0;
        IsPotassiumEnabled = isPotassiumEnabled;

        PhosphorusLimitMg = phosphorusLimitMg > 0 ? phosphorusLimitMg : 850.0;
        IsPhosphorusEnabled = isPhosphorusEnabled;

        SodiumLimitMg = sodiumLimitMg > 0 ? sodiumLimitMg : 1500.0;
        IsSodiumEnabled = isSodiumEnabled;

        CalciumLimitMg = calciumLimitMg > 0 ? calciumLimitMg : 1000.0;
        IsCalciumEnabled = isCalciumEnabled;

        MagnesiumLimitMg = magnesiumLimitMg > 0 ? magnesiumLimitMg : 350.0;
        IsMagnesiumEnabled = isMagnesiumEnabled;

        IronLimitMg = ironLimitMg > 0 ? ironLimitMg : 15.0;
        IsIronEnabled = isIronEnabled;

        ZincLimitMg = zincLimitMg > 0 ? zincLimitMg : 12.0;
        IsZincEnabled = isZincEnabled;

        PreferredLanguage = string.IsNullOrWhiteSpace(preferredLanguage) ? "es" : preferredLanguage.Trim();
        ThemePreference = string.IsNullOrWhiteSpace(themePreference) ? "Dark" : themePreference.Trim();

        CreatedAt = createdAt == default ? DateTime.UtcNow : createdAt;
        UpdatedAt = updatedAt == default ? DateTime.UtcNow : updatedAt;
    }

    /// <summary>
    /// Crea un perfil con las directrices estandar KDOQI y USDA de referencia.
    /// </summary>
    public static UserProfile CreateDefault(string userId = "local_user")
    {
        DateTime now = DateTime.UtcNow;
        return new UserProfile(
            id: Guid.NewGuid(),
            userId: userId,
            displayName: "Usuario Principal",
            email: null,
            dietaryCondition: "Renal / KDOQI",
            weightKg: null,
            heightCm: null,
            birthDate: null,
            biologicalSex: null,
            dailyCalorieTarget: 2000.0,
            isCalorieGoalEnabled: true,
            dailyProteinTargetGrams: 60.0,
            isProteinGoalEnabled: true,
            warningThresholdPercentage: 80.0,
            potassiumLimitMg: 2000.0,
            isPotassiumEnabled: true,
            phosphorusLimitMg: 850.0,
            isPhosphorusEnabled: true,
            sodiumLimitMg: 1500.0,
            isSodiumEnabled: true,
            calciumLimitMg: 1000.0,
            isCalciumEnabled: true,
            magnesiumLimitMg: 350.0,
            isMagnesiumEnabled: true,
            ironLimitMg: 15.0,
            isIronEnabled: true,
            zincLimitMg: 12.0,
            isZincEnabled: true,
            preferredLanguage: "es",
            themePreference: "Dark",
            createdAt: now,
            updatedAt: now);
    }

    public void UpdateProfileDetails(
        string displayName,
        string? email,
        string? dietaryCondition,
        double? weightKg,
        double? heightCm,
        DateTime? birthDate,
        string? biologicalSex)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? DisplayName : displayName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        DietaryCondition = string.IsNullOrWhiteSpace(dietaryCondition) ? DietaryCondition : dietaryCondition.Trim();
        WeightKg = weightKg is > 0 and < 500 ? weightKg : WeightKg;
        HeightCm = heightCm is > 0 and < 300 ? heightCm : HeightCm;
        BirthDate = birthDate ?? BirthDate;
        BiologicalSex = biologicalSex ?? BiologicalSex;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNutritionalGoals(
        double calorieTarget,
        bool isCalorieEnabled,
        double proteinTargetGrams,
        bool isProteinEnabled)
    {
        if (calorieTarget >= 0)
        {
            DailyCalorieTarget = calorieTarget;
        }

        IsCalorieGoalEnabled = isCalorieEnabled;

        if (proteinTargetGrams >= 0)
        {
            DailyProteinTargetGrams = proteinTargetGrams;
        }

        IsProteinGoalEnabled = isProteinEnabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateWarningThreshold(double percentage)
    {
        if (percentage is >= 50.0 and <= 95.0)
        {
            WarningThresholdPercentage = percentage;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateMineralLimit(MineralType mineral, double limitMg, bool isEnabled)
    {
        double validLimit = limitMg > 0 ? limitMg : GetMineralLimit(mineral);

        switch (mineral)
        {
            case MineralType.Potassium:
                PotassiumLimitMg = validLimit;
                IsPotassiumEnabled = isEnabled;
                break;
            case MineralType.Phosphorus:
                PhosphorusLimitMg = validLimit;
                IsPhosphorusEnabled = isEnabled;
                break;
            case MineralType.Sodium:
                SodiumLimitMg = validLimit;
                IsSodiumEnabled = isEnabled;
                break;
            case MineralType.Calcium:
                CalciumLimitMg = validLimit;
                IsCalciumEnabled = isEnabled;
                break;
            case MineralType.Magnesium:
                MagnesiumLimitMg = validLimit;
                IsMagnesiumEnabled = isEnabled;
                break;
            case MineralType.Iron:
                IronLimitMg = validLimit;
                IsIronEnabled = isEnabled;
                break;
            case MineralType.Zinc:
                ZincLimitMg = validLimit;
                IsZincEnabled = isEnabled;
                break;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public double GetMineralLimit(MineralType mineral)
    {
        return mineral switch
        {
            MineralType.Potassium => PotassiumLimitMg,
            MineralType.Phosphorus => PhosphorusLimitMg,
            MineralType.Sodium => SodiumLimitMg,
            MineralType.Calcium => CalciumLimitMg,
            MineralType.Magnesium => MagnesiumLimitMg,
            MineralType.Iron => IronLimitMg,
            MineralType.Zinc => ZincLimitMg,
            _ => 0.0
        };
    }

    public bool IsMineralLimitEnabled(MineralType mineral)
    {
        return mineral switch
        {
            MineralType.Potassium => IsPotassiumEnabled,
            MineralType.Phosphorus => IsPhosphorusEnabled,
            MineralType.Sodium => IsSodiumEnabled,
            MineralType.Calcium => IsCalciumEnabled,
            MineralType.Magnesium => IsMagnesiumEnabled,
            MineralType.Iron => IsIronEnabled,
            MineralType.Zinc => IsZincEnabled,
            _ => false
        };
    }

    public void UpdatePreferences(string preferredLanguage, string themePreference)
    {
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
        {
            PreferredLanguage = preferredLanguage.Trim();
        }

        if (!string.IsNullOrWhiteSpace(themePreference))
        {
            ThemePreference = themePreference.Trim();
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
