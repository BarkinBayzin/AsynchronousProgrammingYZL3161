using AsynchronousProgramming.Models.Entities.Abstract;

namespace AsynchronousProgramming.Models.VMs
{
    public class PageVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public Status Status { get; set; }
    }
}
