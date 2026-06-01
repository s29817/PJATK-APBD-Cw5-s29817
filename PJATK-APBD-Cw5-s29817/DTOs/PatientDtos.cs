namespace PJATK_APBD_Cw5_s29817.DTOs;

public record PatientResponseDto(
    string Pesel,
    string FirstName,
    string LastName,
    int Age,
    string Sex,
    List<AdmissionResponseDto> Admissions,
    List<BedAssignmentResponseDto> BedAssignments
);

public record AdmissionResponseDto(
    int Id,
    DateTime AdmissionDate,
    DateTime? DischargeDate,
    WardResponseDto Ward
);

public record WardResponseDto(
    int Id,
    string Name,
    string Description
);

public record BedAssignmentResponseDto(
    int Id,
    DateTime From,
    DateTime? To,
    BedResponseDto Bed
);

public record BedResponseDto(
    int Id,
    BedTypeResponseDto BedType,
    RoomResponseDto Room
);

public record BedTypeResponseDto(
    int Id,
    string Name,
    string Description
);

public record RoomResponseDto(
    string Id,
    bool HasTv,
    WardResponseDto Ward
);