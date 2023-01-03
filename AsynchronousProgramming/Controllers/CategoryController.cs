using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.DTOs;
using AsynchronousProgramming.Models.Entities.Abstract;
using AsynchronousProgramming.Models.Entities.Concrete;
using AsynchronousProgramming.Models.VMs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(IBaseRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                var slug = await _categoryRepository.GetByDefault(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The category already exists..!");
                    TempData["Warning"] = "The category already exists..!";
                    return View(model);
                }
                else
                {
                    var category = _mapper.Map<Category>(model);
                    await _categoryRepository.Add(category);
                    TempData["Success"] = "The category has been created..!";
                    return RedirectToAction("List");
                }
            }
            else
            {
                TempData["Error"] = "The category hasn't been created..!";
                return View(model);
            }
        }


        public async Task<IActionResult> List()
        {
            var categories = await _categoryRepository.GetFilteredList(select: x => new CategoryVM
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Status = x.Status,
                CreateDate = x.CreateDate
            },
            where: x => x.Status != Status.Passive,
            orderBy: x => x.OrderByDescending(z => z.CreateDate));

            return View(categories);
        }

        public async Task<IActionResult> Remove(int id)
        {
            Category category = await _categoryRepository.GetById(id);

            if(category != null)
            {
                category.Status = Status.Passive;
                category.DeleteDate = System.DateTime.Now;
                await _categoryRepository.Delete(category);
                TempData["Success"] = "The category has been removed..!";
                return RedirectToAction("List");
            }
            else
            {
                TempData["Warning"] = "There is no such a category..!";
                return RedirectToAction("List");
            }
        }

        //GET: https://localhost:5001/Category/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            Category category = await _categoryRepository.GetById(id);
            if (category is null) return RedirectToAction("List");
            UpdateCategoryDTO model = _mapper.Map<UpdateCategoryDTO>(category);

            //UpdateCategoryDTO model2 = new UpdateCategoryDTO
            //{ bir üst satırda kullanılan AutoMapper kütüphanesi sayesinde mapping işlemini tanıtıp, açık açık bu işlemleri yapmak zorunda kalmayız.
            //    Id = category.Id,
            //    Name = category.Name,
            //    Slug = category.Slug
            //}
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                var slug = await _categoryRepository.GetByDefault(x => x.Slug == model.Slug);

                if(slug != null)
                {
                    ModelState.AddModelError(string.Empty, $"{model.Name} already exists..!");
                    TempData["Warning"] = "The category already exists..!";
                    return View(model);
                }
                else
                {
                    Category category = _mapper.Map<Category>(model);
                    await _categoryRepository.Update(category);
                    TempData["Success"] = "The category has been updated..!";
                    return RedirectToAction("List");
                }
            }
            else
            {
                TempData["Error"] = "The category hasn't been updated..!";
                return View(model);
            }
        }
    }
}
