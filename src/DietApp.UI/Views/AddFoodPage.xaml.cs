using BarcodeScanning;
using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Vista de formulario para registrar un alimento nuevo. Aloja la integracion
/// del control CameraView para escaneo de codigos de barra (EAN/UPC) con solicitud asincrona
/// de permisos de hardware en tiempo de ejecucion mediante Methods.AskForRequiredPermissionAsync().
/// Por que se tomo esta decision: Permite encapsular los eventos de plataforma y ciclo de vida de la camara
/// en el code-behind de la vista sin acoplar dependencias de hardware o interfaces graficas al ViewModel.
/// </summary>
public partial class AddFoodPage : ContentPage
{
    public AddFoodPage(AddFoodViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnScanButtonTapped(object? sender, EventArgs e)
    {
        try
        {
            var isPermissionGranted = await Methods.AskForRequiredPermissionAsync();
            if (!isPermissionGranted)
            {
                await DisplayAlertAsync(
                    "Permiso de Cámara",
                    "Se requiere permiso de acceso a la cámara para escanear el código de barras. Puedes escribirlo manualmente en el campo de texto.",
                    "Aceptar");
                return;
            }

            if (BindingContext is AddFoodViewModel viewModel)
            {
                viewModel.StartScanCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Cámara",
                $"No fue posible inicializar la cámara para el escaneo: {ex.Message}",
                "Aceptar");
        }
    }

    private void OnScannerDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        if (e.BarcodeResults == null || e.BarcodeResults.Count == 0) return;

        var detectedResult = e.BarcodeResults.FirstOrDefault();
        if (detectedResult != null)
        {
            string? barcodeValue = !string.IsNullOrWhiteSpace(detectedResult.RawValue)
                ? detectedResult.RawValue
                : detectedResult.DisplayValue;

            if (!string.IsNullOrWhiteSpace(barcodeValue))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is AddFoodViewModel viewModel)
                    {
                        viewModel.OnBarcodeDetected(barcodeValue);
                    }
                });
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is AddFoodViewModel viewModel && viewModel.IsScanning)
        {
            viewModel.StopScanCommand.Execute(null);
        }
    }
}
