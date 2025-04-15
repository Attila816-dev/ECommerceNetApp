using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Category
{
    public record GetAllCategoriesQuery() : IRequest<IEnumerable<CategoryDto>>;
}
