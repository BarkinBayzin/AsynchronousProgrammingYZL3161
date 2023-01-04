using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.DTOs;
using AsynchronousProgramming.Models.Entities.Abstract;
using AsynchronousProgramming.Models.Entities.Concrete;
using AsynchronousProgramming.Models.VMs;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _catRepository.GetByDefaults(x => x.Status != Status.Passive), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDTO model)
        {
            if (ModelState.IsValid)
            {
                string imageName = "noimage.png";
                if(model.UploadImage != null)
                {
                    //Dosyanın kök dizindeki yerini yakalıyorum
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    //Yüklenecek dosyaların aynı dosya olsa bile birbirleri ile çakışmaması için uniqueleştirmeye çalışıyorum burada kullandığımız temp. (Guid_FileName)
                    imageName = $"{Guid.NewGuid()}_{model.UploadImage.FileName}";

                    //Artık elimde yüklenecek dizin ve dosya mevcut, benim bunları fileStream sınıfına göndermek için combine ediyorum
                    string filePath = Path.Combine(uploadDir, imageName);

                    //Yaratılan dosya yolunun kayıt işlemini başlatmak için instance alınıfrken ctor'a parametrelerimi iletiyorum.
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);

                    //ve dosyayı yazma/ekleme/kopyalama işlemi bu satırda tamamlanıyor
                    await model.UploadImage.CopyToAsync(fileStream);
                    //stream'i kapatmazsak hata alırız
                    fileStream.Close(); 
                }

                Product product = _mapper.Map<Product>(model);
                product.Image = imageName;
                await _proRepository.Add(product);
                TempData["Success"] = "The product has been created..!";
                return RedirectToAction("List");
            }
            TempData["Error"] = "The product hasn't been created..!";
            return View();
        }

        public async Task<IActionResult> List()
        {
            var products = await _proRepository.GetFilteredList(
                select: x => new ProductVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryName = x.Category.Name,
                    Image = x.Image,
                    UnitPrice = x.UnitPrice,
                    Status = x.Status,
                    Description = x.Description
                },
                where: x => x.Status != Status.Passive,
                orderBy: x => x.OrderByDescending(x => x.CreateDate),
                join: x => x.Include(x => x.Category)); //eager loading

            return View(products);
        }
    }
}
