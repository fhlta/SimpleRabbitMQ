using Domain.Model;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    public interface IOrderService
    {
        Task<OrderCreateResponse> PostOrderServiceAsync(OrderCreateRequest orderDTO);
    }
}