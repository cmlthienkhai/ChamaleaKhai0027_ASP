﻿using ChamaleaKhai0027.Context;
using System.Linq;
using System.Web.Mvc;

namespace ChamaleaKhai0027.Controllers
{
    public class BrandController : Controller
    {
        private readonly WedsiteEcomEntities1 _context;

        // Constructor để khởi tạo context
        public BrandController()
        {
            _context = new WedsiteEcomEntities1();
        }

        // Action để hiển thị tất cả các thương hiệu
        public ActionResult Index()
        {
            // Lấy danh sách tất cả các thương hiệu
            var brands = _context.Brands.ToList();
            return View(brands);
        }

        // Action để hiển thị các sản phẩm theo thương hiệu cụ thể
        public ActionResult ProductByBrand(int brandId)
        {
            // Lấy thông tin thương hiệu và các sản phẩm liên quan
            var brand = _context.Brands.Find(brandId);
            if (brand == null)
            {
                return HttpNotFound();
            }

            var products = _context.Products.Where(p => p.BrandId == brandId).ToList();

            // Truyền dữ liệu sang View
            ViewBag.Brand = brand;
            return View(products);
        }
    }
}
