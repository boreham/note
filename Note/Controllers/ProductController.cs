using Microsoft.AspNetCore.Mvc;
using Note.Models;
using Note.Repository;

namespace Note.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // Отображение всех продуктов
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllProductsAsync();
        return View(products);
    }

    // Страница создания нового продукта
    public IActionResult Create()
    {
        return View();
    }

    // Добавить новый продукт
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            await _productRepository.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // Страница редактирования продукта
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // Обновить продукт
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _productRepository.UpdateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // Удалить продукт
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // Подтверждение удаления
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productRepository.DeleteProductAsync(id);
        return RedirectToAction(nameof(Index));
    }
}