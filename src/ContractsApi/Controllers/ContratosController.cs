using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.Api.Controllers
{
    [ApiController]
    public class ContratosController : ControllerBase
    {
        private readonly ITimeService _timeService;
        private readonly IMaterialService _materialService;
        private readonly IFornecedorService _fornecedorService;
        private readonly IContratoService _contratoService;

        public ContratosController(
            ITimeService timeService,
            IMaterialService materialService,
            IFornecedorService fornecedorService,
            IContratoService contratoService)
        {
            _timeService = timeService;
            _materialService = materialService;
            _fornecedorService = fornecedorService;
            _contratoService = contratoService;
        }

        [HttpGet("contracts/teams")]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _timeService.GetAll();
            return Ok(teams);
        }

        [HttpGet("contracts/materials")]
        public async Task<IActionResult> GetMaterials()
        {
            var materials = await _materialService.GetAll();
            return Ok(materials);
        }

        [HttpGet("contracts/suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _fornecedorService.GetAll();
            return Ok(suppliers);
        }

        [HttpGet("contracts")]
        public async Task<IActionResult> Get()
        {
            var contracts = await _contratoService.GetAll();
            return Ok(contracts);
        }

        [HttpPost("contracts/teams")]
        public async Task<IActionResult> PostTeams([FromBody] List<TeamDto> teamsDto)
        {
            await _timeService.Upsert(teamsDto);
            return NoContent();
        }

        [HttpPost("contracts/materials")]
        public async Task<IActionResult> PostMaterials([FromBody] List<MaterialDto> materialsDto)
        {
            await _materialService.Upsert(materialsDto);
            return NoContent();
        }

        [HttpPost("contracts/suppliers")]
        public async Task<IActionResult> PostSuppliers([FromBody] List<FornecedorDto> fornecedoresDto)
        {
            await _fornecedorService.Upsert(fornecedoresDto);
            return NoContent();
        }

        [HttpPost("contracts")]
        public async Task<IActionResult> Post([FromBody] List<ContratoDto> contratosDto)
        {
            await _contratoService.Upsert(contratosDto);
            return NoContent();
        }
    }
}