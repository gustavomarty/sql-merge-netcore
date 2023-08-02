using ContractsApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace ContractsApi.Controllers
{
    [ApiController]
    public class ContratosController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;

        public ContratosController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }
    }
}