using System;
using AutoMapper;
using Payments.Core.Models;
using Payments.Web.ViewModels;

namespace Payments.Web.Mappings
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<InvoiceEditViewModel, Invoice>();
        }
    }
}
