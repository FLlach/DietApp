using DietApp.Domain.Entities;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para el almacenamiento de cabeceras de alinos y condimentos.
/// Almacena el identificador unico, el nombre y la descripcion del preparado.
/// Por que se tomo esta decision: Permite indexar y consultar rapidamente los alinos guardados
/// y relacionarlos con sus componentes dosificados a traves de claves foraneas en SeasoningItemEntity.
/// </summary>
[Table("Seasonings")]
public class SeasoningEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Seasoning ToDomain(IEnumerable<SeasoningItem> items)
    {
        return new Seasoning(
            Id,
            Name,
            Description,
            items);
    }

    public static SeasoningEntity FromDomain(Seasoning seasoning)
    {
        return new SeasoningEntity
        {
            Id = seasoning.Id,
            Name = seasoning.Name,
            Description = seasoning.Description
        };
    }
}
