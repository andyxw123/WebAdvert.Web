using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly CognitoUserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            this._signInManager = signInManager;
            this._userManager = userManager as CognitoUserManager<CognitoUser>;
            this._pool = pool;
        }

        public IActionResult Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);

                //Check if user already exists
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exsits");
                    return View(model);
                }

                //Add in other required attribute (using Email Address as Name here)
                user.Attributes.Add(CognitoAttribute.Name.ToString(), model.Email);

                //Create User: If password is not included then a temp password will be allocated by Cognito
                //and the user informed by email so that they can change it.
                var createdUser = await _userManager.CreateAsync(user, model.Password);

                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm", "Accounts", new ConfirmModel { Email = model.Email });
                }

                foreach (var error in createdUser.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }


            return View(model);
        }

        public IActionResult Confirm(ConfirmModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm_Post(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "User not found for this email address");
                    return View(model);
                }

                var result = await _userManager.ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
            }

            return View("Confirm", model);
        }

        public IActionResult Login(LoginModel model)
        {
            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login_Post(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "User not found for this email address");

                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginFailed", "Email and password do not match");
                }
            }

            return View("Login", model);
        }
    }
}
