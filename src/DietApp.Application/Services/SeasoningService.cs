using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Implementa los casos de uso para la creacion, consulta y eliminacion de alinos y condimentos.
/// Resuelve los alimentos de la composicion contra el catalogo, calcula el aporte exacto de compuestos y calorias
/// por gramaje, y persiste el agregado Seasoning a traves de ISeasoningRepository.
/// Por que se tomo esta decision: En DDD, centraliza la logica de negocio de los condimentos en la capa de aplicacion,
/// garantizando que la manipulacion y reincorporacion de alinos a recetas mantenga la integridad matematica y nutricional.
/// </summary>
public class SeasoningService : ISeasoningService
{
    private readonly ISeasoningRepository _seasoningRepository;
    private readonly IFoodRepository _foodRepository;

    public SeasoningService(
        ISeasoningRepository seasoningRepository,
        IFoodRepository foodRepository)
    {
        _seasoningRepository = seasoningRepository ?? throw new ArgumentNullException(nameof(seasoningRepository));
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
    }

    public async Task<IReadOnlyList<SeasoningDto>> GetAllSeasoningsAsync()
    {
        var seasonings = await _seasoningRepository.GetAllAsync();
        return seasonings.Select(s => s.ToDto()).ToList();
    }

    public async Task<SeasoningDto?> GetSeasoningByIdAsync(Guid id)
    {
        var seasoning = await _seasoningRepository.GetByIdAsync(id);
        return seasoning?.ToDto();
    }

    public async Task<SeasoningDto> CreateSeasoningAsync(
        string name,
        string description,
        IEnumerable<(Guid foodItemId, double grams)> items)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del alino es obligatorio.", nameof(name));
        }

        var seasoning = new Seasoning(
            Guid.NewGuid(),
            name,
            description);

        if (items != null)
        {
            foreach (var (foodItemId, grams) in items)
            {
                if (grams <= 0) continue;

                var foodItem = await _foodRepository.GetByIdAsync(foodItemId);
                if (foodItem != null)
                {
                    seasoning.AddItem(SeasoningItem.FromFoodItem(foodItem, grams));
                }
            }
        }

        await _seasoningRepository.SaveAsync(seasoning);
        return seasoning.ToDto();
    }

    public async Task DeleteSeasoningAsync(Guid id)
    {
        await _seasoningRepository.DeleteAsync(id);
    }
}
