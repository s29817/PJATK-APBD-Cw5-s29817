using Microsoft.AspNetCore.Mvc;
using PJATK_APBD_Cw5_s29817.DTOs;
using PJATK_APBD_Cw5_s29817.Exceptions;
using PJATK_APBD_Cw5_s29817.Services;

namespace PJATK_APBD_Cw5_s29817.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPatients(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var patients = await service.GetPatientsAsync(search, cancellationToken);
        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(
        [FromRoute] string pesel,
        [FromBody] AssignBedRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.AssignBedAsync(pesel, request, cancellationToken);

            return Created(
                $"/api/patients/{pesel}/bedassignments/{result.Id}",
                result
            );
        }
        catch (BadRequestException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (NotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
    }
}