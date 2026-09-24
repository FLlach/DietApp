using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel encargado de coordinar la vista de seguimiento diario y conteo de minerales
/// por comidas. Permite navegar entre fechas, visualizar el total acumulado de cada compuesto mineral,
/// listar el desglose de cada ingesta y disparar alertas visuales preventivas si algun mineral
/// excede el limite maximo diario configurado por el usuario en Ajustes.
/// Por que se tomo esta decision: Emplea propiedades parciales de C# 13 con CommunityToolkit.Mvvm,
/// garantizando compatibilidad WinRT y AOT con enlace de datos reactivo y evaluacion inmediata de umbrales.
/// </summary>
public partial class MealTrackingViewModel : ObservableObject
{
    private readonly IMealTrackingService _mealTrackingService;
    private readonly IMineralAlertService _mineralAlertService;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasExceededAlerts { get; set; }

    public ObservableCollection<MealDto> DayMeals { get; } = new();
    public ObservableCollection<MineralAmountDto> DailyMinerals { get; } = new();
    public ObservableCollection<MineralAlertExceededDto> ExceededAlerts { get; } = new();

    public MealTrackingViewModel(
        IMealTrackingService mealTrackingService,
        IMineralAlertService mineralAlertService)
    {
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));
        _mineralAlertService = mineralAlertService ?? throw new ArgumentNullException(nameof(mineralAlertService));

        _mineralAlertService.ThresholdsChanged += async (_, _) =>
        {
            await LoadDayDataAsync();
        };
    }

    [RelayCommand]
    public async Task LoadDayDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var meals = await _mealTrackingService.GetMealsForDateAsync(SelectedDate);
            DayMeals.Clear();
            foreach (var meal in meals)
            {
                DayMeals.Add(meal);
            }

            var totals = await _mealTrackingService.GetDailyMineralTotalsAsync(SelectedDate);
            DailyMinerals.Clear();
            foreach (var total in totals)
            {
                DailyMinerals.Add(total);
            }

            // Evaluar alertas de limites maximos fijados por el usuario
            var alerts = _mineralAlertService.CheckExceededThresholds(DailyMinerals);
            ExceededAlerts.Clear();
            foreach (var alert in alerts)
            {
                ExceededAlerts.Add(alert);
            }
            HasExceededAlerts = ExceededAlerts.Count > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GoToPreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await LoadDayDataAsync();
    }

    [RelayCommand]
    public async Task GoToNextDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await LoadDayDataAsync();
    }

    [RelayCommand]
    public async Task DeleteMealAsync(Guid mealId)
    {
        await _mealTrackingService.DeleteMealAsync(mealId);
        await LoadDayDataAsync();
    }
}
