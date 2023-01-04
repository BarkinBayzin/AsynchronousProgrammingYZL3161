using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.Entities.Concrete;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AsynchronousProgramming.Controllers
{
    public class ProductController : Controller
    {
        private readonly IBaseRepository<Product> _proRepository;
        private readonly IBaseRepository<Category> _catRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment; //Projenin çalıştığı server bilgisi gibi, kök kaynak bilgilerine ulaşmamı sağlayan arayüz.

        public ProductController(IBaseRepository<Product> proRepository, IBaseRepository<Category> catRepository, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _proRepository = proRepository;
            _catRepository = catRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
