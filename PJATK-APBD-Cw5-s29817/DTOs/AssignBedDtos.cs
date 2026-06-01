namespace PJATK_APBD_Cw5_s29817.DTOs;

public record AssignBedRequestDto(
    DateTime From,
    DateTime? To,
    string BedType,
    string Ward
);

public record AssignBedResponseDto(
    int Id,
    string PatientPesel,
    int BedId,
    DateTime From,
    DateTime? To
);