using AsynchronousProgramming.Models.Entities.Abstract;
using System;

namespace AsynchronousProgramming.Models.VMs
{
    public class CategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public Status Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
