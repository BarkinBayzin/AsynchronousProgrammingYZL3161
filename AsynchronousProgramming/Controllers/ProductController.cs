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
using System.Collections.Generic;
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
        public async Task<IActionResult> Index2() => View(await _proRepository.GetByDefaults(p => p.Status != Status.Passive));
        public async Task<IActionResult> ProductByCategory(string categorySlug)
        {
            Category category = await _catRepository.GetByDefault(x => x.Slug == categorySlug);

            if (category == null) return RedirectToAction("List");

            ViewBag.CategoryName = category.Name;
            ViewBag.CategorySlug = category.Slug;

            List<Product> products = await _proRepository.GetByDefaults(x => x.CategoryId == category.Id);

            return View(products);
        }
        public async Task<IActionResult> Remove(int id)
        {
            Product product = await _proRepository.GetById(id);

            if(product != null)
            {
                product.DeleteDate = DateTime.Now;
                product.Status = Status.Passive;
                await _proRepository.Delete(product);
                TempData["Success"] = "The product has been removed";
                return RedirectToAction("List");
            }
            else
            {
                TempData["Error"] = "The product has not been removed..!";
                return RedirectToAction("List");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            Product product = await _proRepository.GetById(id);
            UpdateProductDTO model = new UpdateProductDTO
            {
                Categories = await _catRepository.GetByDefaults(x => x.Status != Status.Passive),
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                UnitPrice = product.UnitPrice,
                Image = product.Image
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(UpdateProductDTO model)
        {
            if(ModelState.IsValid)
            {
                string imageName = "noimage.png";
                if (model.UploadImage != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    //eğer model'in image'i noimage.png değil ise, o zaman kayıt sırasında resim ekleme başarılı olmuş demektir. Ben şu anda bunu update etmek istiyorsam(resmi) eski resmi silmeliyim çünkü gereksiz büyük boyutlarda dosyaları proje içeriisnde barındırmanın lüzümu yoktur.
                    if(!string.Equals(model.Image, "noimage.png"))
                    {
                        //daha öncesinde resim yüklenebilmiş.
                        string oldPath = Path.Combine(uploadDir, model.Image);
                        if(System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    imageName = $"{Guid.NewGuid()}_{model.UploadImage.FileName}";
                    string filePath = Path.Combine(uploadDir, imageName);
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.UploadImage.CopyToAsync(fileStream);
                    fileStream.Close();
                }

                Product product = _mapper.Map<Product>(model);
                product.Image = imageName;
                await _proRepository.Update(product);
                TempData["Success"] = "The product has been edited..!";
                return RedirectToAction("List");
            }
            else
            {
                TempData["Error"] = "The product has been edited..!";
                return View(model);
            }
        }
    }
}
