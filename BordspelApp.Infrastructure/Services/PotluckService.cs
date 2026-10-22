using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;

namespace BordspelApp.Infrastructure.Services;

public class PotluckService : IPotluckService
{
    private readonly IPotluckRepository _potluckRepository;

    public PotluckService(IPotluckRepository potluckRepository)
    {
        _potluckRepository = potluckRepository;
    }

    // Registreert een item voor een deelnemer die aan de spelavond meedoet.
    public async Task<PotluckItemDto?> AddItemAsync(PotluckItemCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return null;
        }

        if (!await _potluckRepository.GameNightExistsAsync(dto.GameNightId) ||
            !await _potluckRepository.IsParticipantAsync(dto.GameNightId, dto.PersonId))
        {
            return null;
        }

        var item = new PotluckItem(
            dto.GameNightId,
            dto.PersonId,
            dto.Description,
            dto.IsLactoseFree,
            dto.IsNutFree,
            dto.IsVegetarian,
            dto.IsAlcoholFree);

        var savedItem = await _potluckRepository.AddAsync(item);
        return Map(savedItem);
    }

    // Haalt alle geregistreerde potluckitems van een spelavond op.
    public async Task<IReadOnlyList<PotluckItemDto>> GetItemsAsync(int gameNightId)
    {
        var items = await _potluckRepository.GetByGameNightAsync(gameNightId);
        return items.Select(Map).ToList();
    }

    // Zet een domeinitem om naar het contract dat de applicatielaag gebruikt.
    private static PotluckItemDto Map(PotluckItem item)
    {
        return new PotluckItemDto
        {
            Id = item.Id,
            Description = item.Description,
            IsLactoseFree = item.IsLactoseFree,
            IsNutFree = item.IsNutFree,
            IsVegetarian = item.IsVegetarian,
            IsAlcoholFree = item.IsAlcoholFree
        };
    }
}
