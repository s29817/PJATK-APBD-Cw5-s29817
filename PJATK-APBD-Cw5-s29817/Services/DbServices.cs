using Microsoft.EntityFrameworkCore;
using PJATK_APBD_Cw5_s29817.Data;
using PJATK_APBD_Cw5_s29817.DTOs;
using PJATK_APBD_Cw5_s29817.Exceptions;
using PJATK_APBD_Cw5_s29817.Models;

namespace PJATK_APBD_Cw5_s29817.Services;

public class DbService(HospitalContext context) : IDbService
{
    public async Task<List<PatientResponseDto>> GetPatientsAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var query = context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern));
        }

        return await query
            .Select(p => new PatientResponseDto(
                p.Pesel,
                p.FirstName,
                p.LastName,
                p.Age,
                p.Sex ? "Male" : "Female",
                p.Admissions
                    .Select(a => new AdmissionResponseDto(
                        a.Id,
                        a.AdmissionDate,
                        a.DischargeDate,
                        new WardResponseDto(
                            a.Ward.Id,
                            a.Ward.Name,
                            a.Ward.Description
                        )
                    ))
                    .ToList(),
                p.BedAssignments
                    .Select(ba => new BedAssignmentResponseDto(
                        ba.Id,
                        ba.From,
                        ba.To,
                        new BedResponseDto(
                            ba.Bed.Id,
                            new BedTypeResponseDto(
                                ba.Bed.BedType.Id,
                                ba.Bed.BedType.Name,
                                ba.Bed.BedType.Description
                            ),
                            new RoomResponseDto(
                                ba.Bed.Room.Id,
                                ba.Bed.Room.HasTv,
                                new WardResponseDto(
                                    ba.Bed.Room.Ward.Id,
                                    ba.Bed.Room.Ward.Name,
                                    ba.Bed.Room.Ward.Description
                                )
                            )
                        )
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssignBedResponseDto> AssignBedAsync(
        string pesel,
        AssignBedRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.To is not null && request.From >= request.To)
        {
            throw new BadRequestException("Date 'from' must be earlier than date 'to'.");
        }

        var patientExists = await context.Patients
            .AnyAsync(p => p.Pesel == pesel, cancellationToken);

        if (!patientExists)
        {
            throw new NotFoundException($"Patient with PESEL '{pesel}' was not found.");
        }

        var bedTypeExists = await context.BedTypes
            .AnyAsync(bt => bt.Name == request.BedType, cancellationToken);

        if (!bedTypeExists)
        {
            throw new NotFoundException($"Bed type '{request.BedType}' was not found.");
        }

        var wardExists = await context.Wards
            .AnyAsync(w => w.Name == request.Ward, cancellationToken);

        if (!wardExists)
        {
            throw new NotFoundException($"Ward '{request.Ward}' was not found.");
        }

        var freeBed = await context.Beds
            .Where(b =>
                b.BedType.Name == request.BedType &&
                b.Room.Ward.Name == request.Ward)
            .Where(b => !context.BedAssignments.Any(ba =>
                ba.BedId == b.Id &&

                // Sprawdzenie nakładania się zakresów dat:
                // istniejące.From < nowe.To
                // nowe.From < istniejące.To
                (request.To == null || ba.From < request.To) &&
                (ba.To == null || request.From < ba.To)
            ))
            .OrderBy(b => b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (freeBed is null)
        {
            throw new NotFoundException(
                $"No free bed of type '{request.BedType}' was found in ward '{request.Ward}' for the given period.");
        }

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = freeBed.Id,
            From = request.From,
            To = request.To
        };

        await context.BedAssignments.AddAsync(assignment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new AssignBedResponseDto(
            assignment.Id,
            assignment.PatientPesel,
            assignment.BedId,
            assignment.From,
            assignment.To
        );
    }
}