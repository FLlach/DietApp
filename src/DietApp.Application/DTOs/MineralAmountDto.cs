using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: Objeto de transferencia de datos (DTO) que expone la informacion de un mineral
/// con su nombre amigable, unidad y valor numerico para las capas superiores (UI).
/// Por que se tomo esta decision: Desacopla la representacion del Value Object del dominio
/// de la capa de interfaz, facilitando el formateo y la localizacion de nombres sin alterar
/// las reglas del nucleo.
/// </summary>
public class MineralAmountDto
{
    public MineralType Type { get; set; }
    public string MineralName { get; set; } = string.Empty;
    public double Milligrams { get; set; }
    public string Unit { get; set; } = "mg";
    public string DisplayText => $"{MineralName}: {Milligrams:F1} {Unit}";
}
