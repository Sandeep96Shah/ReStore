using System;
using API.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace API.Extensions;

public static class ProductExtensions
{
    public static IQueryable<Product> Sort(this IQueryable<Product> query, string? orderBy)
    {
        query = orderBy switch
        {
            "price" => query.OrderBy(x => x.Price),
            "productDesc" => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };
        return query;
    }

    public static IQueryable<Product> Search(this IQueryable<Product> query, string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return query;

        string lowerCaseSearchTerm = searchTerm.Trim().ToLower();
        return query.Where(x => x.Name.ToLower().Contains(lowerCaseSearchTerm));
    }

    public static IQueryable<Product> Filter(this IQueryable<Product> query, string? brands, string? types)
    {
        var brandList = new List<string>();
        var typeList = new List<string>();

        if (!string.IsNullOrEmpty(brands))
            brandList.AddRange(brands.ToLower().Split(",").Select(x => x.Trim()));

        if (!string.IsNullOrEmpty(types))
            typeList.AddRange(types.ToLower().Split(",").Select(x => x.Trim()));
            
        if(brandList.Count > 0)
        {
            query = query.Where(x => brandList.Contains(x.Brand.ToLower()));
        }

        if (typeList.Count > 0)
        {
            query = query.Where(x => typeList.Contains(x.Type.ToLower()));
        }

        return query;
    }
}
