using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Payment;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class PaymentMappers
{
    internal static partial PaymentLineResponse ToResponse(this Entities.SalePayment p);
}
