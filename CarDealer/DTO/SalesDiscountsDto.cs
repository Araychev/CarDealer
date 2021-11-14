namespace CarDealer.DTO
{
   public class SalesDiscountsDto
    {
        public CarDto car { get; set; }

        public string customerName { get; set; }
        public string Discount { get; set; }
        public string price { get; set; }
        public string priceWithDiscount { get; set; }
    }
}
