using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para almacenar las recetas marcadas como favoritas por cada usuario/perfil.
/// Mantiene indices por UserId y RecipeId para busquedas rapidas de favoritos.
/// Por que se tomo esta decision: Permite persistir los favoritos de forma local y offline por perfil.
/// </summary>
[Table("UserRecipeFavorites")]
public class UserRecipeFavoriteEntity
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Indexed]
    public string UserId { get; set; } = string.Empty;

    [Indexed]
    public Guid RecipeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
