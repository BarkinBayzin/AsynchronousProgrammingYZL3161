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
    public class PageController : Controller
    {
        private readonly IBaseRepository<Page> _pageRepository;
        private readonly IMapper _mapper;

        public PageController(IBaseRepository<Page> pageRepository, IMapper mapper)
        {
            _pageRepository = pageRepository;
            _mapper = mapper;
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreatePageDTO model)
        {
            if (ModelState.IsValid) //model içerisindeki üyelere koyulan kurallara uyduk mu?
            {
                //Model'den gelen slug veri tabanında var mı yok mu diye baktık.
                var slug = await _pageRepository.GetByDefault(x => x.Slug == model.Slug);

                if(slug != null)//slug null değilse, veri tabanında böyle bir slug var demektir. O halde ekleme işlemi gerçekleşmemeli, şayet ekleme gerçekleşirse birden fazla aynı varlıktan oluşur.
                {
                    ModelState.AddModelError("", "The page is already exists..!");
                    TempData["Warning"] = "The page is already exists..!";
                    return View(model);
                }
                else
                {
                    //Veri tabanındaki page tablosuna sadece "page" tipinde veri ekleyebiliriz. Bu action methoda gelen verinin tipi "CreatePageDTO" olduğundan direkt veri tabanındaki tabloya ekleme gerçekleştiremeyiz. Bu yüzden DTO'dan gelen veriyi AutoMapper 3rd party tool aracı ile Page varlığının üyelerine eşliyoruz.
                    Page page = _mapper.Map<Page>(model);
                    //kullanıcadan gelen data model ile buraya taşındı ve Page tipindeki page objesine dolduruldu artık veri tabanına ekleyebiliriz.
                    await _pageRepository.Add(page);
                    TempData["Success"] = "The page has been created..!";
                    return RedirectToAction("List");
                }
            }else
            {
                TempData["Error"] = "the page hasn't been created..!";
                return View(model);
            }
        }

        public async Task<IActionResult> List()
        {
            var pages = await _pageRepository.GetFilteredList(
                select: x => new PageVM
                {
                    Id = x.Id,
                    Content = x.Content,
                    Slug = x.Slug,
                    Status = x.Status,
                    Title = x.Title
                },
                where: x => x.Status != Status.Passive,
                orderBy: x => x.OrderByDescending(x => x.Id));

            return View(pages);
        }

        public async Task<IActionResult> Edit(int id)
        {
            Page page = await _pageRepository.GetById(id);
            if (page == null) return RedirectToAction("List");
            UpdatePageDTO model = _mapper.Map<UpdatePageDTO>(page);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdatePageDTO model)
        {
            if(ModelState.IsValid)
            {
                var slug = await _pageRepository.GetByDefault(x => x.Slug == model.Slug);

                if(slug != null)
                {
                    ModelState.AddModelError("", "The page is already exists..!");
                    TempData["Warning"] = "The page is already exists..!";
                    return View(model);
                }
                else
                {
                    Page page = _mapper.Map<Page>(model);
                    await _pageRepository.Update(page);
                    TempData["Success"] = "The page has been updated..!";
                    return RedirectToAction("List");
                }
            }
            else
            {
                TempData["Error"] = "The page has not been updated..!";
                return View(model);
            }
        }


        public async Task<IActionResult> Page(string slug)
        {
            if(slug == null) return View(await _pageRepository.GetByDefault(x => x.Slug == "ana-sayfa"));//parametreden gelen slug boş ise ana sayfayı aç

            var page = await _pageRepository.GetByDefault(x => x.Slug == slug); //parametreden gelen slug ne ise o sluga ait sayfayı ilgili değişkene atadık.

            if (page == null) return NotFound();//yukarıdaki linq to null dönerse bulundamadı sayfası burada client'a dönülür.

            return View(page);//slug vasıtasıyla yakalanan sayfa dönülür.
        }

        public async Task<IActionResult> Remove(int id)
        {
            Page page =await _pageRepository.GetById(id);
            if(page != null)
            {
                page.Status = Status.Passive;
                page.DeleteDate = System.DateTime.Now;
                await _pageRepository.Delete(page);
                TempData["Success"] = "The page has been removed..!";
                return RedirectToAction("List");
            }
            else
            {
                TempData["Error"] = "The page has not been removed..!";
                return RedirectToAction("List");
            }
        }
    }
}
