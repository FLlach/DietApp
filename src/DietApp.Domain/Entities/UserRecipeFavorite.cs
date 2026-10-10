namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Entidad de dominio que representa la asociacion de una receta marcada como favorita por un perfil de usuario.
/// Mantiene la relacion entre el identificador del perfil/usuario y el identificador de la receta con su fecha de creacion.
/// Por que se tomo esta decision: En DDD, independiza la nocion de favoritos del motor de persistencia,
/// permitiendo asociar recetas locales y globales a un perfil especifico tanto en SQLite como en Supabase.
/// </summary>
public class UserRecipeFavorite
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public Guid RecipeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public UserRecipeFavorite(Guid id, string userId, Guid recipeId, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("El identificador del usuario no puede estar vacio.", nameof(userId));
        }

        if (recipeId == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la receta no puede ser vacio.", nameof(recipeId));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId.Trim();
        RecipeId = recipeId;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}
