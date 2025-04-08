using AutoMapper;
using GameService.Base;
using GameService.Data;
using GameService.DTOs;
using GameService.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameService.Repositories.ForCategory;

public class CategoryRepository : ICategoryRepository
{
    private readonly GameDbContext _context;
    private IMapper _mapper;
    private BaseResponseModel _response;
    public CategoryRepository(GameDbContext context, IMapper mapper, BaseResponseModel response)
    {
        _context = context;
        _mapper = mapper;
        _response = response;
    }

    public async Task<BaseResponseModel> CreateCategory(CategoryDTO category)
    {
        var objDto=_mapper.Map<Category>(category); //dönüşüm eşleme {category name ve description tutar bunları objye verdik}
        await _context.Categories.AddAsync(objDto); //name ve desc bilgisini ekledik categ tablosuna

        if(await _context.SaveChangesAsync() > 0) //ekleme işlemi başarılıysa dbde değişiklik olduysa true(1) olur
        {
            _response.IsSuccess=true;
            _response.Message="Category adding successful";
            _response.Data=objDto;
            return _response;
        }
        _response.Message="Category adding unsuccessful";
        _response.IsSuccess=false;
        return _response;

    }

    public async Task<BaseResponseModel> GetAllCategories()
    {
        List<Category> categories=await _context.Categories.ToListAsync(); // list tipinde olmalı categories veya var olmalı
        if(categories is not null){
            _response.Data=categories;
            _response.IsSuccess=true;
            return _response;
        }
        _response.IsSuccess=false;
        return _response;
    }

    public async Task<bool> RemoveCategory(Guid categoryId)
    {
        Category category= await _context.Categories.FindAsync(categoryId); //id aldık

        if(category is not null)
        {
             _context.Categories.Remove(category);
             if(await _context.SaveChangesAsync()>0)
             {  
                return true;

             }
        }
        return false;
    }

    public async Task<BaseResponseModel> UpdateCategory(CategoryDTO category,Guid categoryId)
    {
        Category obj=await _context.Categories.FindAsync(categoryId);
        
        if(obj is not null)
        {
            obj.CategoryName=category.CategoryName;
            obj.CategoryDescription=category.CategoryDescription; 

            if(await _context.SaveChangesAsync()>0){
                _response.Data=category;
                _response.Message="Success";
                _response.IsSuccess=true;
                return _response;
            }
        }
        _response.IsSuccess=false;
        return _response;
        
    }
}