using ShopApp.DataAccess.Abstract;
using ShopApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopApp.DataAccess
{
    public interface ICategoryDal : IRepository<Category>
    {
        Category GetByIdWithProducts(int id);
        void DeleteFromCategory(int categoryId, int productId);
    }
}
