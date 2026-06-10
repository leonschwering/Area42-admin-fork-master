namespace Area42_1.ApiService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Area42_1.ApiService.Services;
using Area42_1.ApiService.Models.Reservations;
using Area42_1.ApiService.Data.Repositories;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IAccommodationRepository _accommodationRepository;

    public ReservationsController(IReservationService service, IAccommodationRepository accommodationRepository)
    {
        _service = service;
        _accommodationRepository = accommodationRepository;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReservations(Guid userId)
    {
        var reservations = await _service.GetUserReservationsAsync(userId);
        return Ok(reservations);
    }

    [HttpGet("accommodation/{accommodationId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAccommodationReservations(Guid accommodationId)
    {
        var reservations = await _service.GetAccommodationReservationsAsync(accommodationId);
        return Ok(reservations);
    }

    [HttpGet("availability/check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] Guid accommodationId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var isAvailable = await _service.IsAvailableAsync(accommodationId, checkIn, checkOut);
        return Ok(new { available = isAvailable });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var reservation = await _service.GetReservationAsync(id);
        if (reservation == null)
            return NotFound();

        return Ok(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reservation = await _service.CreateReservationAsync(request, _accommodationRepository);
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Reservation reservation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _service.UpdateReservationAsync(id, reservation);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _service.CancelReservationAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
