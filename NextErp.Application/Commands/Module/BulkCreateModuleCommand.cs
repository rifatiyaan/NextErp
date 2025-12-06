using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands.Module
{
    public class BulkCreateModuleCommand : IRequest<List<int>>
    {
        public List<BulkModuleDto> Modules { get; set; } = new();
    }
}
