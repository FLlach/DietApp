using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Domain.ValueObjects;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Implementa los casos de uso del catalogo de alimentos. Orquesta las llamadas
/// al repositorio de dominio para filtrar por rangos de minerales o terminos de busqueda,
/// y transforma los resultados a DTOs para la interfaz de usuario.
/// Por que se tomo esta decision: Separa la responsabilidad de coordinacion de la UI y del dominio.
/// Asegura que las validaciones de entrada y la transformacion de modelos se ejecuten en un solo lugar.
/// </summary>
public class FoodCatalogService : IFoodCatalogService
{
    private readonly IFoodRepository _foodRepository;

    public FoodCatalogService(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
    }

    public async Task<IReadOnlyList<FoodItemDto>> GetAllFoodsAsync()
    {
        var foods = await _foodRepository.GetAllAsync();
        return foods.Select(food => food.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<FoodItemDto>> FilterFoodsAsync(MineralFilterCriteriaDto criteria)
    {
        if (criteria == null)
        {
            return await GetAllFoodsAsync();
        }

        IReadOnlyList<FoodItem> initialList;

        if (criteria.FilterByProtein)
        {
            double minProtein = criteria.MinimumProteinGrams ?? 0.0;
            double maxProtein = criteria.MaximumProteinGrams ?? double.MaxValue;
            initialList = await _foodRepository.FilterByProteinRangeAsync(minProtein, maxProtein);
        }
        else if (criteria.SelectedMineral.HasValue)
        {
            double min = criteria.MinimumMilligrams ?? 0.0;
            double max = criteria.MaximumMilligrams ?? double.MaxValue;
            initialList = await _foodRepository.FilterByMineralRangeAsync(criteria.SelectedMineral.Value, min, max);
        }
        else
        {
            initialList = await _foodRepository.GetAllAsync();
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            string term = criteria.SearchTerm.Trim().ToLowerInvariant();
            initialList = initialList
                .Where(food => food.Name.ToLowerInvariant().Contains(term) ||
                               food.Category.ToLowerInvariant().Contains(term))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
        {
            initialList = criteria.SortBy switch
            {
                "ProteinDesc" => initialList.OrderByDescending(f => f.ProteinGrams).ThenBy(f => f.Name).ToList(),
                "ProteinAsc" => initialList.OrderBy(f => f.ProteinGrams).ThenBy(f => f.Name).ToList(),
                "NameAsc" => initialList.OrderBy(f => f.Name).ToList(),
                _ => initialList
            };
        }

        return initialList.Select(food => food.ToDto()).ToList();
    }

    public async Task<FoodItemDto?> GetFoodByIdAsync(Guid id)
    {
        var food = await _foodRepository.GetByIdAsync(id);
        return food?.ToDto();
    }

    public async Task SaveFoodAsync(FoodItemDto foodDto)
    {
        if (foodDto == null)
        {
            throw new ArgumentNullException(nameof(foodDto));
        }

        var minerals = foodDto.Minerals.Select(dto => new MineralAmount(dto.Type, dto.Milligrams)).ToList();
        var food = new FoodItem(
            foodDto.Id,
            foodDto.Name,
            foodDto.Category,
            foodDto.ReferenceGrams,
            minerals,
            foodDto.Calories,
            foodDto.ProteinGrams);

        var existing = await _foodRepository.GetByIdAsync(food.Id);
        if (existing == null)
        {
            await _foodRepository.AddAsync(food);
        }
        else
        {
            await _foodRepository.UpdateAsync(food);
        }
    }

    public async Task DeleteFoodAsync(Guid id)
    {
        await _foodRepository.DeleteAsync(id);
    }
}
