using System.Collections.Generic;

namespace CarDealer.DTO
{
   public class CarPartsExportDto
    {
        public CarDto car { get; set; }
        public ICollection<PartDto> parts { get; set; } = new List<PartDto>();
    }
}
