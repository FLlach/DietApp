using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `user_recipe_favorites` en Supabase PostgreSQL.
/// Asocia el identificador de usuario de Supabase Auth con las recetas guardadas como favoritas.
/// Por que se tomo esta decision: Permite sincronizar los favoritos del perfil de usuario en la nube con politicas RLS activas.
/// </summary>
[Table("user_recipe_favorites")]
public class SupabaseRecipeFavoriteModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("recipe_id")]
    public string RecipeId { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
