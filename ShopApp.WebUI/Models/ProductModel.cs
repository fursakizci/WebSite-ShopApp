using ShopApp.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopApp.WebUI.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
       // [Required]
        //[StringLength(60,MinimumLength =10,ErrorMessage ="On karakterden fazla giriniz!")]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "On karakterden fazla giriniz!")]
        public string Description { get; set; }
        [Required(ErrorMessage ="Fiyat giriniz!")]
        [Range(1,1000)]
        public decimal? Price { get; set; }
        public List<Category> SelectedCategories { get; set; }
    }
}
