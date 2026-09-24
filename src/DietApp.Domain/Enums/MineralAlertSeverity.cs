namespace DietApp.Domain.Enums;

/// <summary>
/// Como funciona: Define los niveles de severidad para las alertas de consumo de minerales:
/// aviso preventivo al aproximarse al limite configurado por el usuario o alerta por haberlo superado.
/// Por que se tomo esta decision: Permite discriminar visualmente y logicamente advertencias tempranas
/// de excesos efectivos, facilitando la toma preventiva de decisiones nutricionales antes de superar los limites.
/// </summary>
public enum MineralAlertSeverity
{
    NearLimit = 1,
    ExceededLimit = 2
}
