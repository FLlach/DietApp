using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel encargado de coordinar la vista de seguimiento diario y conteo de minerales
/// por comidas. Permite navegar entre fechas, visualizar el total acumulado de cada compuesto mineral
/// y listar el desglose de cada ingesta.
/// Por que se tomo esta decision: Emplea propiedades parciales de C# 13 con CommunityToolkit.Mvvm,
/// garantizando compatibilidad WinRT y AOT con enlace de datos reactivo y eficiente.
/// </summary>
public partial class MealTrackingViewModel : ObservableObject
{
    private readonly IMealTrackingService _mealTrackingService;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<MealDto> DayMeals { get; } = new();
    public ObservableCollection<MineralAmountDto> DailyMinerals { get; } = new();

    public MealTrackingViewModel(IMealTrackingService mealTrackingService)
    {
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));
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
