namespace Area42_1.ApiService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Area42_1.ApiService.Services;
using Area42_1.ApiService.Models.Accommodations;

[ApiController]
[Route("api/[controller]")]
public class AccommodationsController : ControllerBase
{
    private readonly IAccommodationService _service;

    public AccommodationsController(IAccommodationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accommodations = await _service.GetAllAccommodationsAsync();
        return Ok(accommodations);
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(AccommodationType type)
    {
        var accommodations = await _service.GetAccommodationsByTypeAsync(type);
        return Ok(accommodations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var accommodation = await _service.GetAccommodationAsync(id);
        if (accommodation == null)
            return NotFound();

        return Ok(accommodation);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Accommodation accommodation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateAccommodationAsync(accommodation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Accommodation accommodation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        accommodation.Id = id;

        try
        {
            var updated = await _service.UpdateAccommodationAsync(accommodation);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAccommodationAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
