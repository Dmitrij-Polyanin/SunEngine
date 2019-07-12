using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SunEngine.Core.Filters;
using SunEngine.Core.Presenters;

namespace SunEngine.Core.Controllers
{
    public class SearchController : BaseController
    {
        protected readonly ISearchPresenter searchPresenter;
        
        public SearchController(ISearchPresenter searchPresenter ,IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.searchPresenter = searchPresenter;
        }

        // TODO : Add check max and min searchPattern length
        [IpSpamProtectionFilter]
        [HttpPost]
        public async Task<IActionResult> SearchUsers(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)|| (searchString.Length<3)||(searchString.Length>20)) return BadRequest();

            return Ok(await searchPresenter.SearchByUsernameOrLink(searchString));
        }
    }
}