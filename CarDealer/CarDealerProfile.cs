using AutoMapper;
using CarDealer.DTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<SuppliersImportDto, Supplier>();

            CreateMap<PartsImportDto, Part>()
                .ForMember(x => x.SupplierId,
                    y 
                        => y.MapFrom(z => z.supplierId));
        }
    }
}
