using AutoMapper;
using Payments.Core.Models;
using Payments.Models;

namespace Payments.Mappings
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<InvoiceEditViewModel, Invoice>()
                .ForMember(d => d.Id, opt => opt.Ignore());
        }
    }
}
