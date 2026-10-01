using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Servicio de aplicacion que orquesta la consulta, persistencia y actualizacion del perfil del usuario.
/// Asegura la existencia de un perfil predeterminado si el almacenamiento esta vacio y propaga reactivamente
/// los cambios hacia los servicios satelite de metas caloricas, proteicas, alertas de minerales, idioma y tema visual.
/// Por que se tomo esta decision: Establece una Fuente Unica de Verdad (Single Source of Truth) para las preferencias
/// del paciente, facilitando la auditoria local y preparando la transicion transparente hacia tablas remotas en Supabase.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICalorieGoalService _calorieGoalService;
    private readonly IProteinGoalService _proteinGoalService;
    private readonly IMineralAlertService _mineralAlertService;
    private readonly ILocalizationService _localizationService;
    private readonly IThemeService _themeService;

    public event EventHandler<UserProfileDto>? ProfileChanged;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        ICalorieGoalService calorieGoalService,
        IProteinGoalService proteinGoalService,
        IMineralAlertService mineralAlertService,
        ILocalizationService localizationService,
        IThemeService themeService)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _calorieGoalService = calorieGoalService ?? throw new ArgumentNullException(nameof(calorieGoalService));
        _proteinGoalService = proteinGoalService ?? throw new ArgumentNullException(nameof(proteinGoalService));
        _mineralAlertService = mineralAlertService ?? throw new ArgumentNullException(nameof(mineralAlertService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
    }

    public async Task<UserProfileDto> GetCurrentProfileAsync()
    {
        var profile = await _userProfileRepository.GetProfileAsync();
        if (profile == null)
        {
            profile = UserProfile.CreateDefault();
            await _userProfileRepository.SaveProfileAsync(profile);
        }

        SyncServicesWithProfile(profile);
        return profile.ToDto();
    }

    public async Task<UserProfileDto> SaveProfileAsync(UserProfileDto profileDto)
    {
        if (profileDto == null)
        {
            throw new ArgumentNullException(nameof(profileDto));
        }

        var domainProfile = profileDto.ToDomain();
        await _userProfileRepository.SaveProfileAsync(domainProfile);

        SyncServicesWithProfile(domainProfile);

        var resultDto = domainProfile.ToDto();
        ProfileChanged?.Invoke(this, resultDto);
        return resultDto;
    }

    public async Task UpdateProfileDetailsAsync(
        string displayName,
        string? email,
        string? dietaryCondition,
        double? weightKg,
        double? heightCm,
        DateTime? birthDate,
        string? biologicalSex)
    {
        var profile = await GetOrCreateDomainProfileAsync();
        profile.UpdateProfileDetails(displayName, email, dietaryCondition, weightKg, heightCm, birthDate, biologicalSex);

        await _userProfileRepository.SaveProfileAsync(profile);
        ProfileChanged?.Invoke(this, profile.ToDto());
    }

    public async Task UpdateNutritionalGoalsAsync(
        double calorieTarget,
        bool isCalorieEnabled,
        double proteinTargetGrams,
        bool isProteinEnabled)
    {
        var profile = await GetOrCreateDomainProfileAsync();
        profile.UpdateNutritionalGoals(calorieTarget, isCalorieEnabled, proteinTargetGrams, isProteinEnabled);

        await _userProfileRepository.SaveProfileAsync(profile);
        _calorieGoalService.SetDailyCalorieGoal(calorieTarget, isCalorieEnabled);
        _proteinGoalService.SetDailyProteinGoal(proteinTargetGrams, isProteinEnabled);

        ProfileChanged?.Invoke(this, profile.ToDto());
    }

    public async Task UpdateWarningThresholdAsync(double thresholdPercent)
    {
        var profile = await GetOrCreateDomainProfileAsync();
        profile.UpdateWarningThreshold(thresholdPercent);

        await _userProfileRepository.SaveProfileAsync(profile);
        _mineralAlertService.SetWarningPercentage(thresholdPercent);

        ProfileChanged?.Invoke(this, profile.ToDto());
    }

    public async Task UpdateMineralLimitAsync(MineralType mineral, double limitMg, bool isEnabled)
    {
        var profile = await GetOrCreateDomainProfileAsync();
        profile.UpdateMineralLimit(mineral, limitMg, isEnabled);

        await _userProfileRepository.SaveProfileAsync(profile);

        double? thresholdValue = isEnabled ? limitMg : null;
        _mineralAlertService.SetThreshold(mineral, thresholdValue);

        ProfileChanged?.Invoke(this, profile.ToDto());
    }

    public async Task UpdatePreferencesAsync(string preferredLanguage, string themePreference)
    {
        var profile = await GetOrCreateDomainProfileAsync();
        profile.UpdatePreferences(preferredLanguage, themePreference);

        await _userProfileRepository.SaveProfileAsync(profile);

        if (!string.IsNullOrWhiteSpace(preferredLanguage))
        {
            _localizationService.SetLanguage(preferredLanguage);
        }

        if (Enum.TryParse<ThemeMode>(themePreference, true, out var themeMode))
        {
            _themeService.SetTheme(themeMode);
        }

        ProfileChanged?.Invoke(this, profile.ToDto());
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaultProfile = UserProfile.CreateDefault();
        await _userProfileRepository.SaveProfileAsync(defaultProfile);

        SyncServicesWithProfile(defaultProfile);
        ProfileChanged?.Invoke(this, defaultProfile.ToDto());
    }

    private async Task<UserProfile> GetOrCreateDomainProfileAsync()
    {
        var profile = await _userProfileRepository.GetProfileAsync();
        if (profile == null)
        {
            profile = UserProfile.CreateDefault();
            await _userProfileRepository.SaveProfileAsync(profile);
        }
        return profile;
    }

    private void SyncServicesWithProfile(UserProfile profile)
    {
        _calorieGoalService.SetDailyCalorieGoal(profile.DailyCalorieTarget, profile.IsCalorieGoalEnabled);
        _proteinGoalService.SetDailyProteinGoal(profile.DailyProteinTargetGrams, profile.IsProteinGoalEnabled);
        _mineralAlertService.SetWarningPercentage(profile.WarningThresholdPercentage);

        var mineralThresholds = new Dictionary<MineralType, double?>();
        mineralThresholds[MineralType.Potassium] = profile.IsPotassiumEnabled ? profile.PotassiumLimitMg : null;
        mineralThresholds[MineralType.Phosphorus] = profile.IsPhosphorusEnabled ? profile.PhosphorusLimitMg : null;
        mineralThresholds[MineralType.Sodium] = profile.IsSodiumEnabled ? profile.SodiumLimitMg : null;
        mineralThresholds[MineralType.Calcium] = profile.IsCalciumEnabled ? profile.CalciumLimitMg : null;
        mineralThresholds[MineralType.Magnesium] = profile.IsMagnesiumEnabled ? profile.MagnesiumLimitMg : null;
        mineralThresholds[MineralType.Iron] = profile.IsIronEnabled ? profile.IronLimitMg : null;
        mineralThresholds[MineralType.Zinc] = profile.IsZincEnabled ? profile.ZincLimitMg : null;

        _mineralAlertService.SaveThresholds(mineralThresholds);
    }
}
