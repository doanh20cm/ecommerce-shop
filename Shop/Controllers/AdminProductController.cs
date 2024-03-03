﻿using Application;
using Application.Brands;
using Application.Categories;
using Application.Helper;
using Application.Products;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string ImageFolder = "ProductImages";

        public AdminProductController
        (
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var model = new ProductListingPageModel
            {
                Categories = _categoryService.GetCategories(),
                Brands = _brandService.GetBrands(),
                OrderBys = EnumHelper.GetList(typeof(SortEnum))
            };
            return View(model);
        }

        public IActionResult Create()
        {
            var model = new ProductListingPageModel
            {
                Categories = _categoryService.GetCategories(),
                Brands = _brandService.GetBrands(),
            };
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New product data invalided!"));
                return RedirectToAction("Create");
            }
            try
            {
                model.ImageUrls = await SaveImage(model.Images);
                await _productService.CreateProduct(model);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Add {model.ProductName} successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, $"Create product failed!"));   
                return RedirectToAction("Create");
            }      
        }

        public IActionResult ProductListPartial([FromBody] ProductPage model)
        {
            var result = _productService.GetProducts(model);
            return PartialView(result);
        }

        private async Task<List<string>> SaveImage(List<IFormFile> images)
        {
            var imageLinks = new List<string>();
            foreach (var image in images)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
                string productImageDir = Path.Combine(uploadFolder, ImageFolder);
                if (!Directory.Exists(productImageDir))
                {
                    Directory.CreateDirectory(productImageDir);
                }
                string fileName = $"{Guid.NewGuid()}-{image.FileName}";
                string fileUrl = $"/{ShopConstants.UploadFolder}/{ImageFolder}/{fileName}";
                using var stream = new FileStream(Path.Combine(productImageDir, fileName), FileMode.Create);
                await image.CopyToAsync(stream);
                imageLinks.Add(fileUrl);
            }
            return imageLinks;
        }
    }
}
