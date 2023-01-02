using AsynchronousProgramming.Models.Entities.Abstract;
using System.Collections.Generic;

namespace AsynchronousProgramming.Models.Entities.Concrete
{
    public class Category:BaseEntity
    {
        public string Name { get; set; } //ElectronicDevices
        public string Slug { get; set; } //electronic-devices

        //Navigation Properties
        public List<Product> Products { get; set; }
    }
}
