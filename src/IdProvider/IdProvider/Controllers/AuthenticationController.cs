using IdProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            AuthenticationModel viewModel = new AuthenticationModel();
            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Index(AuthenticationModel viewModel)
        {

          
            return View(viewModel);
        }




        private Uri BuildRedirectUri(AuthenticationModel model)
        {
            Uri uri = new Uri()

        }
    }
}
