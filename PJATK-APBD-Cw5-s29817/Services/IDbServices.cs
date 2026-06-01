using PJATK_APBD_Cw5_s29817.DTOs;

namespace PJATK_APBD_Cw5_s29817.Services;

public interface IDbService
{
    Task<List<PatientResponseDto>> GetPatientsAsync(
        string? search,
        CancellationToken cancellationToken
    );

    Task<AssignBedResponseDto> AssignBedAsync(
        string pesel,
        AssignBedRequestDto request,
        CancellationToken cancellationToken
    );
}