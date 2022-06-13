using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ASPNETIdentityManager.Models;
using Microsoft.AspNetCore.Identity;
using ASPNETIdentityManager.Entities;
using ASPNETIdentityManager.Contexts;
using ASPNETIdentityManager.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ASPNETIdentityManager.DB;

namespace ASPNETIdentityManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly Repository repository;

        public HomeController(ILogger<HomeController> logger, Repository repository)
        {
            this.logger = logger;
            this.repository = repository;
        }
        

        public IActionResult Index([FromServices] UserDBContext dBContext, string userName, string email)
        {
            UsersAndRolesViewModel model = new UsersAndRolesViewModel();
            model.Users = dBContext.Users.Where(u =>
            (string.IsNullOrEmpty(userName) ? u.UserName != null : u.UserName.Contains(userName))
            &&
            (string.IsNullOrEmpty(email) ? true : u.Email != null && u.Email.Contains(email))
            ).Select(u => new User()
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                UserRoles = (from r in dBContext.Roles
                             join ur in dBContext.UserRoles.Where(ur => ur.UserId == u.Id) on r.Id equals ur.RoleId
                             select new Role()
                             {
                                 IdentityRole = r,
                                 RoleClaims = dBContext.RoleClaims.Where(rc => rc.RoleId == r.Id).ToList()
                             }).ToList(),
                UserClaims = dBContext.UserClaims.Where(uc => uc.UserId == u.Id).ToList()
            }).ToList();
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult GestisciPrenotazioni([FromServices] UserDBContext dBContext)
        {
            List<Prenotazione> prenotazione = this.repository.GetPrenotazioni();//new List<Prenotazione>();  // //qui deve collegarsi alla tabella nel DB
            List<PrenotazioneModel> model = new List<PrenotazioneModel>();
            foreach (Prenotazione p in prenotazione) model.Add(new PrenotazioneModel()
            {
                Id = p.Id,
                IdUser = p.IdUser,
                Dal = p.Dal,
                Al = p.Al,
                Persone = p.Persone,
                Pacchetto = p.Pacchetto,
                FirstName = (from u in dBContext.Users where p.IdUser.ToString().Equals(u.Id) select u.FirstName).First().ToString(),
                LastName = (from u in dBContext.Users where p.IdUser.ToString().Equals(u.Id) select u.LastName).First().ToString()
            }) ;

            return View(model);
        }
        [HttpPost]
        public IActionResult Prenota([FromBody] Prenotazione prenotazione, [FromServices] UserDBContext dBContext, [FromServices] PrenotazioneDBContext prenotazioneDBContext)
        {
            var user = dBContext.Users.FirstOrDefault(u => u.UserName == prenotazione.User);

            if (user != null)
                prenotazione.IdUser = Guid.Parse(user.Id);

            prenotazioneDBContext.Add(prenotazione);
            prenotazioneDBContext.SaveChanges();

            //qui bisogna implementare l'insert
            return Json("ok");
        }

        
        [HttpPost]
        public async Task<IActionResult> Registrati([FromServices] UserManager<User> userManager, UsersAndRolesViewModel usersViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await userManager.FindByEmailAsync(usersViewModel.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            FirstName = usersViewModel.FirstName,
                            LastName = usersViewModel.LastName,
                            UserName = usersViewModel.UserName,
                            Email = usersViewModel.Email
                        };
                        IdentityResult result = await userManager.CreateAsync(user, usersViewModel.Password);
                        if (result.Succeeded)
                            return LocalRedirect("/");;

                        string errors = string.Empty;
                        foreach (IdentityError error in result.Errors)
                            errors += error.Code + ": " + error.Description + "\n";
                        return Json(errors);
                    }
                    else
                        return Json("Email is already taken");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return Json("Invalid request");
        }       

        [HttpPost]
        public async Task<IActionResult> Login([FromServices] SignInManager<User> signInManager, [FromServices] UserManager<User> userManager, UsersAndRolesViewModel usersViewModel)
        {
            try
            {
                User user = await userManager.FindByNameAsync(usersViewModel.UserName);
                if (user != null)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(usersViewModel.UserName, usersViewModel.Password, true, lockoutOnFailure: false);
                    if (result.Succeeded)
                        return LocalRedirect("/");
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Login");
                        return LocalRedirect("/");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User doesn't exist");
                    return LocalRedirect("/");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return LocalRedirect("/");
        }
        [Authorize]
        public async Task<IActionResult> Logout([FromServices] SignInManager<User> signInManager)
        {
            try
            {
                if (signInManager.IsSignedIn(User))
                {
                    await signInManager.SignOutAsync();
                    return LocalRedirect("/");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return LocalRedirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public ActionResult pacchettoGold()
        {
            return View();
        }
        public ActionResult pacchettoPlatinum()
        {
            return View();
        }
        public ActionResult pacchettoArgento()
        {
            return View();
        }
    }
}
