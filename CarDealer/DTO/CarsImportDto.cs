using System.Collections.Generic;

namespace CarDealer.DTO
{
    public class CarsImportDto
    {

        public string make { get; set; }
        public string model { get; set; }
        public long travelledDistance { get; set; }
        public ICollection<int> partsId { get; set; } = new List<int>();
    }
}
