using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;

namespace BudgetApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}