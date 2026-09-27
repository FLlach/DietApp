using DietApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DietApp.UI;

/// <summary>
/// Como funciona: Clase raiz de la aplicacion MAUI. Inicializa los componentes declarativos XAML
/// e invoca la inicializacion del servicio de tema visual para aplicar de forma inmediata el modo oscuro o claro guardado.
/// Por que se tomo esta decision: Asegura que desde el primer ciclo de renderizado, antes de dibujar AppShell,
/// la propiedad UserAppTheme se encuentre establecida evitando parpadeos visuales al abrir la aplicacion.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
	public App(IThemeService themeService)
	{
		InitializeComponent();
		themeService.Initialize();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}