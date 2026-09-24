using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para el almacenamiento y consulta de recetas culinarias.
/// Administra la persistencia de las cabeceras de recetas, sus ingredientes y los pasos secuenciales numerados con imagenes.
/// Por que se tomo esta decision: Cumple con el contrato IRecipeRepository y asegura que todas las recetas
/// creadas por el usuario se conserven de forma permanente en la base de datos local SQLite.
/// </summary>
public class SqliteRecipeRepository : IRecipeRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteRecipeRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        await _dbContext.InitializeAsync();

        var recipeEntities = await _dbContext.Connection.Table<RecipeEntity>()
            .OrderBy(r => r.Title)
            .ToListAsync();

        var recipes = new List<Recipe>(recipeEntities.Count);
        foreach (var entity in recipeEntities)
        {
            var ingredients = await _dbContext.Connection.Table<RecipeIngredientEntity>()
                .Where(i => i.RecipeId == entity.Id)
                .ToListAsync();

            var steps = await _dbContext.Connection.Table<RecipeStepEntity>()
                .Where(s => s.RecipeId == entity.Id)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            recipes.Add(entity.ToDomain(
                ingredients.Select(i => i.ToDomain()),
                steps.Select(s => s.ToDomain())));
        }

        return recipes;
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        var entity = await _dbContext.Connection.Table<RecipeEntity>().FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return null;

        var ingredients = await _dbContext.Connection.Table<RecipeIngredientEntity>()
            .Where(i => i.RecipeId == id)
            .ToListAsync();

        var steps = await _dbContext.Connection.Table<RecipeStepEntity>()
            .Where(s => s.RecipeId == id)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();

        return entity.ToDomain(
            ingredients.Select(i => i.ToDomain()),
            steps.Select(s => s.ToDomain()));
    }

    public async Task<IReadOnlyList<Recipe>> SearchByTitleAsync(string query)
    {
        await _dbContext.InitializeAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        string pattern = $"%{query.Trim()}%";
        string sql = "SELECT * FROM Recipes WHERE Title LIKE ? OR Description LIKE ? ORDER BY Title ASC";
        var recipeEntities = await _dbContext.Connection.QueryAsync<RecipeEntity>(sql, pattern, pattern);

        var recipes = new List<Recipe>(recipeEntities.Count);
        foreach (var entity in recipeEntities)
        {
            var ingredients = await _dbContext.Connection.Table<RecipeIngredientEntity>()
                .Where(i => i.RecipeId == entity.Id)
                .ToListAsync();

            var steps = await _dbContext.Connection.Table<RecipeStepEntity>()
                .Where(s => s.RecipeId == entity.Id)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            recipes.Add(entity.ToDomain(
                ingredients.Select(i => i.ToDomain()),
                steps.Select(s => s.ToDomain())));
        }

        return recipes;
    }

    public async Task SaveAsync(Recipe recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        await _dbContext.InitializeAsync();

        var recipeEntity = RecipeEntity.FromDomain(recipe);

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            var existing = conn.Find<RecipeEntity>(recipe.Id);
            if (existing != null)
            {
                conn.Update(recipeEntity);
                conn.Execute("DELETE FROM RecipeIngredients WHERE RecipeId = ?", recipe.Id);
                conn.Execute("DELETE FROM RecipeSteps WHERE RecipeId = ?", recipe.Id);
            }
            else
            {
                conn.Insert(recipeEntity);
            }

            foreach (var ing in recipe.Ingredients)
            {
                var ingredientEntity = RecipeIngredientEntity.FromDomain(recipe.Id, ing);
                conn.Insert(ingredientEntity);
            }

            foreach (var step in recipe.Steps)
            {
                var stepEntity = RecipeStepEntity.FromDomain(recipe.Id, step);
                conn.Insert(stepEntity);
            }
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            conn.Execute("DELETE FROM RecipeIngredients WHERE RecipeId = ?", id);
            conn.Execute("DELETE FROM RecipeSteps WHERE RecipeId = ?", id);
            conn.Delete<RecipeEntity>(id);
        });
    }
}
