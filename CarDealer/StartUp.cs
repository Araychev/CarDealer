using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            // context.Database.EnsureDeleted();
          //   context.Database.EnsureCreated();

           //  string suppliersJson = File.ReadAllText("../../../Datasets/suppliers.json");
           //  var suppliersResult = ImportSuppliers(context, suppliersJson);

          //  string partsJson = File.ReadAllText("../../../Datasets/parts.json");
          //   var partsResult = ImportParts(context, partsJson);

          //   string carsJson = File.ReadAllText("../../../Datasets/cars.json");
          //    var carsResult = ImportCars(context, carsJson);


          //  string customersJson = File.ReadAllText("../../../Datasets/customers.json");
          //  var customersResult = ImportCustomers(context, customersJson);

          //  string salesJson = File.ReadAllText("../../../Datasets/sales.json");
          //  var salesResult = ImportCustomers(context, salesJson);

            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var mappConfig = new MapperConfiguration(cfg
                => cfg.AddProfile<CarDealerProfile>());
            var mapper = mappConfig.CreateMapper();

            var suppliersDto = JsonConvert.DeserializeObject<ICollection<SuppliersImportDto>>(inputJson);
            var suppliers = mapper.Map<ICollection<Supplier>>(suppliersDto);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var mappConfig = new MapperConfiguration(cfg
                => cfg.AddProfile<CarDealerProfile>());
            var mapper = mappConfig.CreateMapper();

            var partsDto = JsonConvert.DeserializeObject<ICollection<PartsImportDto>>(inputJson);
            var partsAll = mapper.Map<ICollection<Part>>(partsDto);

            var suppliersId = context
                .Suppliers
                .Select(s => s.Id)
                .ToList();

            var parts = partsAll
                .Where(p => suppliersId.Contains(p.SupplierId))
                .ToList();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carsDto = JsonConvert.DeserializeObject<ICollection<CarsImportDto>>(inputJson);

            List<Car> cars = new List<Car>();

            foreach (var carDto in carsDto)
            {
                Car currentCar = new Car
                {
                    Make = carDto.make,
                    Model = carDto.model,
                    TravelledDistance = carDto.travelledDistance
                };

                foreach (var dtoPartsId in carDto.partsId.Distinct())
                {
                    currentCar.PartCars.Add(new PartCar
                    {
                        PartId = dtoPartsId
                    });
                }
                cars.Add(currentCar);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
           
            var customersDto = JsonConvert.DeserializeObject<ICollection<CustomersImportDto>>(inputJson);

            var customers = customersDto
                .Select(c => new Customer()
                {
                    Name = c.name,
                    BirthDate = c.birthDate,
                    IsYoungDriver = c.isYoungDriver
                }).ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var salesDto = JsonConvert.DeserializeObject<ICollection<SalesImportDto>>(inputJson);

            var sales = salesDto
                .Select(s => new Sale()
                {
                    CarId = s.carId,
                    CustomerId = s.customerId,
                    Discount = s.discount
                }).ToArray();

            context.Sales.AddRange(sales);
            context.SaveChanges();


          return $"Successfully imported {sales.Length}.";
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context
                .Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new CustomersExportDto()
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                }).ToArray();

            return JsonConvert.SerializeObject(customers, Formatting.Indented);
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var carsFromMakeToyota = context
                .Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(m => m.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new
                {
                    c.Id,
                    c.Make,
                    c.Model,
                    c.TravelledDistance
                }).ToArray();

            return JsonConvert.SerializeObject(carsFromMakeToyota, Formatting.Indented);
        }


        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context
                .Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    PartsCount = s.Parts.Count
                }).ToArray();

            return JsonConvert.SerializeObject(localSuppliers, Formatting.Indented);
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {

            var listOfParts = context
                .Cars
                .Select(c => new CarPartsExportDto()
                {
                    car = new CarDto()
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TravelledDistance = c.TravelledDistance
                    },
                    parts = c.PartCars
                        .Select(x => new PartDto()
                        {
                            Name = x.Part.Name,
                            Price = x.Part.Price.ToString("F2")
                        }).ToList()
                }).ToList();

            return JsonConvert.SerializeObject(listOfParts, Formatting.Indented);

        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var salesDto = context.Customers
                .Where(s => s.Sales.Count > 0)
                .Select(s => new SalesExportDto
                {
                    fullName = s.Name,
                    boughtCars = s.Sales.Count,
                    spentMoney = s.Sales.Select(x => x.Car.PartCars.Sum(y => y.Part.Price)).Sum()
                })
                .OrderByDescending(se => se.spentMoney)
                .ThenByDescending(se => se.boughtCars)
                .ToList();

            return JsonConvert.SerializeObject(salesDto, Formatting.Indented);
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var salesDiscount = context
                .Sales.Take(10)
                .Select(s => new SalesDiscountsDto()
                {
                    car = new CarDto()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("F2"),
                    price = s.Car.PartCars.Sum(x => x.Part.Price).ToString("F2"),
                    priceWithDiscount = (s.Car.PartCars
                        .Sum(x => x.Part.Price) * (1.0M - s.Discount / 100)).ToString("F2")
                })
                .ToList();

            return JsonConvert.SerializeObject(salesDiscount, Formatting.Indented);
        }
    }
    
}