﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Infrastructure;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Controllers
{
    public class PagesController : Controller
    {
        private readonly OnlineShopContext context;

        public PagesController(OnlineShopContext context)
        {
            this.context = context;
        }

        //GET  /(root) or /slug
        public async Task<IActionResult> Page(string slug)
        {
            if(slug == null)
            {
                return View(await context.Pages.Where(x => x.Slug == "home").FirstOrDefaultAsync());   
            }
            Page page = await context.Pages.Where(x => x.Slug == slug).FirstOrDefaultAsync();
            if(page == null)
            {
                return NotFound();
            }
            return View(page);
        }
    }
}
