using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.DTOs;
using AsynchronousProgramming.Models.Entities.Concrete;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
                    return RedirectToAction("List","Category");
                }
            }else
            {
                TempData["Error"] = "the page hasn't been created..!";
                return View(model);
            }
        }

    }
}
