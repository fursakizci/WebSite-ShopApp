using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Business.Abstract;
using ShopApp.DataAccess.Abstract;
using ShopApp.Entities;
using ShopApp.WebUI.Extensions;
using ShopApp.WebUI.Identity;
using ShopApp.WebUI.Models;

namespace ShopApp.WebUI.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IEmailSender _emailSender;
        private ICartService _cartService;
        public AccountController(UserManager<ApplicationUser>userManager, SignInManager<ApplicationUser>signInManager, IEmailSender emailSender,ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cartService = cartService;
        }
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                await _emailSender.SendEmailAsync(model.Email, "Hesabinizi Onaylayiniz.", $"Hesabinizi onaylamak icin linke<a href='http://localhost:2829{callbackUrl}' tiklayiniz.</a>");

                TempData.Put("message", new ResultMessageModel()
                {
                    Title = "Hesap Onayi",
                    Message = "E-postaniza gelen link ile hesabinizi onaylayiniz.",
                    Css = "warning"
                });

                _cartService.InitializeCart(user.Id);
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Bilinmeyen bir hata olustu.");

            return View(model);
        }
        public IActionResult Login(string ReturnUrl = null)
        {
            return View(new LoginModel() 
            {
            ReturnUrl = ReturnUrl
            });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                ModelState.AddModelError("","Girdiginiz bilgilere ait kullanici olusturulmamistir.");
                return View(model);
            }
            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //{
            //    ModelState.AddModelError("", "Hesabinizi email ile onaylayiniz ");
            //    return View(model);
            //}

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "~/");
            }
            ModelState.AddModelError("", "Girdiginiz kullanici adi yada paralo hatali!");
            
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new ResultMessageModel()
            {
                Title = "Oturum kapatildi",
                Message = "Hesabiniz guvenli bir sekilde sonlandirildi",
                Css = "warning"
            });
            return Redirect("~/");
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData.Put("message", new ResultMessageModel()
                {
                    Title = "Hesap Onayi",
                    Message = "Hesap onayi icin bilgileriniz yanlis.",
                    Css = "danger"
                });
                return Redirect("~/");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    //Create Cart Object
                    TempData.Put("message", new ResultMessageModel()
                    {
                        Title = "Hesap Onayi",
                        Message = "Hesabiniz basariyla onaylanmistir.",
                        Css = "success"
                    });
                   
                    return RedirectToAction("Login");
                }
            }

            TempData.Put("message", new ResultMessageModel()
            {
                Title = "Hesap Onayi",
                Message = "Hesabiniz onaylanmamistir.",
                Css = "danger"
            });
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData.Put("message", new ResultMessageModel()
                {
                    Title = "Forgot Password",
                    Message = "Bilgileriniz Hatali",
                    Css = "danger"
                });
                return View();
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                TempData.Put("message", new ResultMessageModel()
                {
                    Title = "Forgot Password",
                    Message = "Eposta adresi ile bir kullanici bulunamadi.",
                    Css = "danger"
                });
                return View();
            }
            var code = _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new
            {
                token = code
            });


            await _emailSender.SendEmailAsync(Email, "Reset Password.", $"Parolanizi yenilemek icin <a href='http://localhost:2829{callbackUrl}' tiklayiniz.</a>");
            TempData.Put("message", new ResultMessageModel()
            {
                Title = "Forgot Password",
                Message = "Parola yenilemek icin hesabiniza mail gonderildi.",
                Css = "warning"
            });
            return RedirectToAction("Login", "Account");
           
        }

        public IActionResult ResetPassword(string token)
        {
            if(token == null)
            {
                return RedirectToAction("Home","Index");
            }
            var model = new ResetPasswordModel()
            {
                Token = token
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return RedirectToAction("Home", "Index");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }
        public IActionResult Accessdenied()
        {
            return View();
        }

    }
}