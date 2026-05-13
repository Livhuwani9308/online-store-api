using AutoMapper;
using online_store_api.Models.Category;
using online_store_api.Models.Product;
using online_store_api.Models.User;

namespace online_store_api.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductSize, ProductSizeDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<User, UserDto>();
        }
    }
}