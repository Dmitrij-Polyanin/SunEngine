using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SunEngine.Commons.Models;
using SunEngine.Commons.Services;
using SunEngine.Security.Authentication;
using SunEngine.Stores;

namespace SunEngine.Controllers
{
    public abstract class BaseController : Controller 
    {
        protected readonly MyUserManager userManager;
        protected readonly IUserGroupStore userGroupStore;
        
        protected BaseController(IUserGroupStore userGroupStore, MyUserManager userManager)
        {
            this.userGroupStore = userGroupStore;
            this.userManager = userManager;
        }

        private MyClaimsPrincipal _user;
        
        public new MyClaimsPrincipal User
        {
            get
            {
                if (_user == null)
                {
                    MyClaimsPrincipal myClaimsPrincipal = base.User as MyClaimsPrincipal;
                    _user = myClaimsPrincipal ?? new MyClaimsPrincipal(base.User,userGroupStore);
                }

                return _user;
            }
        }

        public Task<User> GetUserAsync()
        {
            return userManager.FindByIdAsync(User.UserId.ToString());
        }
    }

    public class ErrorViewModel
    {
        public string ErrorName { get; set; }
        public string ErrorText { get; set; }
        public string[] ErrorsNames { get; set; }
        public string[] ErrorsTexts { get; set; }
    }    
}