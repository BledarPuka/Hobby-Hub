using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecondExamBledar.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace SecondExamBledar.Controllers;

public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? UserId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if(UserId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("Auth", "Home", null);
        }
    }
}

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context; 

    public HomeController(ILogger<HomeController> logger, MyContext context)    
    {        
        _logger = logger;
        _context = context;    
    } 


    [HttpGet("Auth")]
    public IActionResult Auth()
    {
        
        return View();
    }

    [HttpPost("Register")]
    public IActionResult Register(User RegisterForm)
    {
            
        if (ModelState.IsValid)
        {   
            PasswordHasher<User> Hasher = new PasswordHasher<User>();        
            RegisterForm.Password = Hasher.HashPassword(RegisterForm, RegisterForm.Password); 
            _context.Add(RegisterForm);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UserId", RegisterForm.UserId);
            return RedirectToAction("Index");
        }
        return View("Auth");

    }

    [HttpPost("Login")]
    public IActionResult Login(Login LoginForm)
    {
            
        if (ModelState.IsValid)
        {   
            User UserDB = _context.Users.FirstOrDefault(e=> e.UserName == LoginForm.LoginUserName);
            if(UserDB == null)        
            {                       
                ModelState.AddModelError("LoginEmail", "Invalid Email");            
                return View("Auth");        
            } 
        
            PasswordHasher<Login> hasher = new PasswordHasher<Login>(); 
            var result = hasher.VerifyHashedPassword(LoginForm, UserDB.Password, LoginForm.LoginPassword);
            if(result == 0)        
            {            
                ModelState.AddModelError("LoginPassword", "Invalid Password");            
                return View("Auth");  
            }
            HttpContext.Session.SetInt32("UserId", UserDB.UserId);
            return RedirectToAction("Index");
            
        }
        return View("Auth");

    }
    
    [HttpGet("Logout")]
    public IActionResult Logout(){
        HttpContext.Session.Clear();
        return RedirectToAction("Auth");
    }

    [SessionCheck]
    public IActionResult Index()
    {
        List<Hobby> AllHobbies = _context.Hobbies.Include(e => e.HobbyCreator).Include(e => e.EnthusiastList).ThenInclude(e => e.UserEnthusiast).OrderByDescending(e => e.EnthusiastList.Count()).ToList();

        ViewBag.AllHobbies = AllHobbies;


        Func<string, List<string>> TopHobbiesByProficiency = (proficiency) =>
        {
            var HobbiesProficiency = _context.Enthusiasts
                .Include(e => e.HobbyEnthusiast)
                .Where(e => e.Proficiency == proficiency);

            var ProficiencyTable = new Dictionary<string, int>();

            foreach (var item in HobbiesProficiency)
            {
                if (!ProficiencyTable.ContainsKey(item.HobbyEnthusiast.Name))
                {
                    ProficiencyTable[item.HobbyEnthusiast.Name] = 0;
                }
                ProficiencyTable[item.HobbyEnthusiast.Name]++;
            }

            var result = new List<string>();

            foreach (var item in ProficiencyTable)
            {
                if (result.Count == 0 || item.Value == ProficiencyTable[result[0]])
                {
                    result.Add(item.Key);
                }
                else if (item.Value > ProficiencyTable[result[0]])
                {
                    result = new List<string>
                    {
                    item.Key
                    };
                }
            }


            return result;
        };
        ViewBag.topNovice = TopHobbiesByProficiency("Novice");
        ViewBag.topIntermediate = TopHobbiesByProficiency("Intermediate");
        ViewBag.topExpert = TopHobbiesByProficiency("Expert");


        return View();
    }

    [SessionCheck]
    [HttpGet("Hobbies/new")]
    public IActionResult CreateHobby()
    {
        return View();
    }

    [SessionCheck]
    [HttpPost]

    public IActionResult HobbyValidation(Hobby hobbyForm)
    {
        if (ModelState.IsValid)
        {
            hobbyForm.UserId = HttpContext.Session.GetInt32("UserId");
            _context.Add(hobbyForm);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("CreateHobby");
    }

    [SessionCheck]
    [HttpGet("Hobbies/edit/{itemid}")]
    public IActionResult EditHobby(int itemid)
    {
        Hobby EditHobby = _context.Hobbies.FirstOrDefault(e => e.HobbyId == itemid);
        return View(EditHobby);
    }

    [SessionCheck]
    [HttpPost]
    public IActionResult UpdateHobby(Hobby UpdateHobby, int itemid)
    {
        Hobby Hobby = _context.Hobbies.FirstOrDefault(e => e.HobbyId == itemid);

        if (ModelState.IsValid)
        {
            Hobby.Name = UpdateHobby.Name;
            Hobby.Description = UpdateHobby.Description;
            Hobby.UpdatedAt = UpdateHobby.UpdatedAt;
            _context.SaveChanges();
            return RedirectToAction("HobbyDetails", new{itemid = Hobby.HobbyId});
        }
        return View("EditHobby");
    }

    [SessionCheck]
    [HttpGet("Hobbies/{itemid}")]
    public IActionResult HobbyDetails(int itemid)
    {
        ViewBag.userId =  HttpContext.Session.GetInt32("UserId");
        
        Hobby AllHobbies = _context.Hobbies.Include(e => e.HobbyCreator).Include(e => e.EnthusiastList).ThenInclude(e => e.UserEnthusiast).FirstOrDefault(e => e.HobbyId == itemid);

        return View(AllHobbies);
    }

    [SessionCheck]
    [HttpPost]
    public IActionResult AddToHobbies(Enthusiast enthusiast, int itemid, string Proficiency)
    {
        User user = _context.Users.FirstOrDefault(e => e.UserId == HttpContext.Session.GetInt32("UserId"));

        bool IsUserAnEnthusiast = _context.Enthusiasts.Any(e => e.UserId == user.UserId && e.HobbyId == itemid);

        if (!IsUserAnEnthusiast)
        {    
            enthusiast.HobbyId = itemid;
            enthusiast.UserId = HttpContext.Session.GetInt32("UserId");
            enthusiast.Proficiency = Proficiency;
            _context.Add(enthusiast);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
