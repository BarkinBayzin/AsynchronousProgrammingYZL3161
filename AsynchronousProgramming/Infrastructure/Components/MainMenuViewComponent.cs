using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.Entities.Abstract;
using AsynchronousProgramming.Models.Entities.Concrete;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Infrastructure.Components
{
    public class MainMenuViewComponent : ViewComponent
    {
        private readonly IBaseRepository<Page> _pageRepository;

        public MainMenuViewComponent(IBaseRepository<Page> pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync() => View(await _pageRepository.GetByDefaults(x => x.Status != Status.Passive));
    }
}
