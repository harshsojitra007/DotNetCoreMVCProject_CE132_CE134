﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Infrastructure;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Areas.Admin.Controllers
{
    //[Authorize(Roles = "admin, editors")]

    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductsController(OnlineShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        //GET admin/categories
        public async Task<IActionResult> Index(int p = 1)
        {
            // Mean how many items per page
            int pageSize = 6;
            var products = context.Products.OrderByDescending(x => x.Id)
                                            .Include(x => x.category)
                                            .Skip((p - 1) * pageSize)
                                            .Take(pageSize);

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Products.Count() / pageSize);
            return View(await products.ToListAsync());
        }

        //GET request   path => /admin/products/create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        //POST request   path => /admin/products/create
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await context.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Page Already Exists.");
                    return View(product);
                }

                string imageName = "noimage.png";
                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                }
                product.Image = imageName;


                context.Add(product);
                await context.SaveChangesAsync();
                TempData["Success"] = "The Product has been added!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        //GET request   path => /admin/products/details/5
        public async Task<IActionResult> Details(int id)
        {
            Product product = await context.Products.Include(x => x.category).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        //GET request   path => /admin/pages/edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Product product = await context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        //POST request   path => /admin/products/edit/4
        public async Task<IActionResult> Edit(int id, Product product)
        {
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name", product.CategoryId);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await context.Products.Where(x => x.Id != id).FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Page Already Exists.");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    if (!string.Equals(product.Image, "noimage.png"))
                    {
                        string oldImagePath = Path.Combine(uploadDir, product.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }


                context.Update(product);
                await context.SaveChangesAsync();
                TempData["Success"] = "The Product has been edited!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        //GET request   path => /admin/products/delete/5
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "The product doest not exists!!";
            }
            else
            {
                if (!string.Equals(product.Image, "noimage.png"))
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    string oldImagePath = Path.Combine(uploadDir, product.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                TempData["Success"] = "The product has been deleted successfully!";
            }

            return RedirectToAction("Index");
        }

    }
}
