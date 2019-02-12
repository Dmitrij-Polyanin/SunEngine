using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using SunEngine.DataBase;
using SunEngine.Services;

namespace SunEngine.Presenters
{
    public class PersonalPresenter : DbService
    {
        public PersonalPresenter(DataBaseConnection db) : base(db)
        {
        }

        public Task<MyUserInfoViewModel> GetMyUserInfoAsync(int id)
        {
            return db.Users.Where(x => x.Id == id).Select(x =>
                new MyUserInfoViewModel
                {
                    Photo = x.Photo,
                    Avatar = x.Avatar,
                    Link = x.Link
                }).FirstOrDefaultAsync();
        }

        public Task<MyProfileInformationViewModel> GetMyProfileInformationAsync(int id)
        {
            return db.Users.Where(x => x.Id == id).Select(x =>
                new MyProfileInformationViewModel
                {
                    Information = x.Information
                }).FirstOrDefaultAsync();
        }
        
        public Task<UserInfoViewModel[]> GetBanListAsync(int userId)
        {
            return db.UserBanedUnits.Where(x => x.UserId == userId).OrderBy(x=>x.UserBaned.UserName).Select(x => 
                new UserInfoViewModel
                {
                    Id = x.UserBaned.Id,
                    Name = x.UserBaned.UserName,
                    Link = x.UserBaned.Link
                }).ToArrayAsync();
        }
    }

    public class MyProfileInformationViewModel
    {
        public string Information { get; set; }
    }

    public class MyUserInfoViewModel
    {
        public string Photo { get; set; }
        public string Avatar { get; set; }
        public string Link { get; set; }
    }
}