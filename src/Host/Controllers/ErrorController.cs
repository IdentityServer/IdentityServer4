﻿using Host.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Host.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IUserInteractionService _interaction;

        public ErrorController(IUserInteractionService interaction)
        {
            _interaction = interaction;
        }

        public async Task<IActionResult> Index(string errorId)
        {
            var vm = new ErrorViewModel();

            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;
            }

            return View("Error", vm);
        }
    }
}
