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
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.History, opt => opt.Ignore())                
                .ForMember(d => d.LineItems, opt => opt.Ignore())
                .ForMember(d => d.Scrubbers, opt => opt.Ignore());

            CreateMap<Invoice, InvoiceEditViewModel>();
        }
    }
}
