using Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly ILogger _logger;

        public OrderController(ILogger logger, IOrderService service)
        {
            _logger = logger;
            _service = service;
        }
        /// <summary>
        /// Post a order
        /// </summary>
        /// <param name="orderRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(OrderCreateRequest orderRequest)
        {
            try
            {
                await _service.PostOrderServiceAsync(orderRequest);
                return Ok(orderRequest);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
