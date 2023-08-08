using Contracts.Data.Data;
using Contracts.Data.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Contracts.Service.Interfaces;
using Contracts.Service.Extensions;

namespace Contracts.Api.Controllers
{
    [ApiController]
    public class AuxiliarController : ControllerBase
    {
        private ITimeService _timeService;
        private IMaterialService _materialService;
        private IFornecedorService _fornecedorService;
        private IContratoService _contratoService;
        private readonly ApplicationContext _applicationContext;
        public readonly string teamsJson = @"[{""nome"":""Outros"",""abreviacao"":""OUT"",""apelido"":"""",""nome_fantasia"":""Outros""},{""nome"":""Ipatinga"",""abreviacao"":""IPA"",""apelido"":"""",""nome_fantasia"":""Ipatinga""},{""nome"":""Cuiabá"",""abreviacao"":""CUI"",""apelido"":""Dourado"",""nome_fantasia"":""Cuiabá""},{""nome"":""Icasa"",""abreviacao"":""ICA"",""apelido"":"""",""nome_fantasia"":""Icasa""},{""nome"":""Oeste"",""abreviacao"":""OES"",""apelido"":"""",""nome_fantasia"":""Oeste""},{""nome"":""DuquedeCaxias"",""abreviacao"":""DUQ"",""apelido"":"""",""nome_fantasia"":""DuquedeCaxias""},{""nome"":""Americana"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Americana""},{""nome"":""GrêmioPrudente"",""abreviacao"":""PRU"",""apelido"":"""",""nome_fantasia"":""GrêmioPrudente""},{""nome"":""Luverdense"",""abreviacao"":""LUV"",""apelido"":"""",""nome_fantasia"":""Luverdense""},{""nome"":""Flamengo"",""abreviacao"":""FLA"",""apelido"":""Mengão"",""nome_fantasia"":""Flamengo""},{""nome"":""Botafogo"",""abreviacao"":""BOT"",""apelido"":""Glorioso"",""nome_fantasia"":""Botafogo""},{""nome"":""Corinthians"",""abreviacao"":""COR"",""apelido"":""Timão"",""nome_fantasia"":""Corinthians""},{""nome"":""Bahia"",""abreviacao"":""BAH"",""apelido"":""TricolordeAço"",""nome_fantasia"":""Bahia""},{""nome"":""Fluminense"",""abreviacao"":""FLU"",""apelido"":""Tricolor"",""nome_fantasia"":""Fluminense""},{""nome"":""Vasco"",""abreviacao"":""VAS"",""apelido"":""GigantedaColina"",""nome_fantasia"":""Vasco""},{""nome"":""Palmeiras"",""abreviacao"":""PAL"",""apelido"":""Verdão"",""nome_fantasia"":""Palmeiras""},{""nome"":""SãoPaulo"",""abreviacao"":""SAO"",""apelido"":""Tricolor"",""nome_fantasia"":""SãoPaulo""},{""nome"":""Santos"",""abreviacao"":""SAN"",""apelido"":""Peixe"",""nome_fantasia"":""Santos""},{""nome"":""Portuguesa"",""abreviacao"":""POR"",""apelido"":"""",""nome_fantasia"":""Portuguesa""},{""nome"":""Guarani"",""abreviacao"":""GUA"",""apelido"":"""",""nome_fantasia"":""Guarani""},{""nome"":""Bragantino"",""abreviacao"":""BGT"",""apelido"":""MassaBruta"",""nome_fantasia"":""Bragantino""},{""nome"":""Atlético-MG"",""abreviacao"":""CAM"",""apelido"":""Galo"",""nome_fantasia"":""Atlético-MG""},{""nome"":""Cruzeiro"",""abreviacao"":""CRU"",""apelido"":""Raposa"",""nome_fantasia"":""Cruzeiro""},{""nome"":""Grêmio"",""abreviacao"":""GRE"",""apelido"":""Imortal"",""nome_fantasia"":""Grêmio""},{""nome"":""Internacional"",""abreviacao"":""INT"",""apelido"":""Colorado"",""nome_fantasia"":""Internacional""},{""nome"":""Juventude"",""abreviacao"":""JUV"",""apelido"":"""",""nome_fantasia"":""Juventude""},{""nome"":""Vitória"",""abreviacao"":""VIT"",""apelido"":"""",""nome_fantasia"":""Vitória""},{""nome"":""Criciúma"",""abreviacao"":""CRI"",""apelido"":"""",""nome_fantasia"":""Criciúma""},{""nome"":""Paraná"",""abreviacao"":""PAR"",""apelido"":"""",""nome_fantasia"":""Paraná""},{""nome"":""Goiás"",""abreviacao"":""GOI"",""apelido"":""Esmeraldino"",""nome_fantasia"":""Goiás""},{""nome"":""Paysandu"",""abreviacao"":""PAY"",""apelido"":"""",""nome_fantasia"":""Paysandu""},{""nome"":""Sport"",""abreviacao"":""SPT"",""apelido"":"""",""nome_fantasia"":""Sport""},{""nome"":""Athlético-PR"",""abreviacao"":""CAP"",""apelido"":""Furacão"",""nome_fantasia"":""Athlético-PR""},{""nome"":""Coritiba"",""abreviacao"":""CFC"",""apelido"":""Coxa"",""nome_fantasia"":""Coritiba""},{""nome"":""Botafogo-SP"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-SP""},{""nome"":""PontePreta"",""abreviacao"":""PON"",""apelido"":"""",""nome_fantasia"":""PontePreta""},{""nome"":""BoaEsporte"",""abreviacao"":""BEC"",""apelido"":"""",""nome_fantasia"":""BoaEsporte""},{""nome"":""SantoAndré"",""abreviacao"":""SAN"",""apelido"":"""",""nome_fantasia"":""SantoAndré""},{""nome"":""SãoBento"",""abreviacao"":""SBE"",""apelido"":"""",""nome_fantasia"":""SãoBento""},{""nome"":""BrasildePelotas"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""BrasildePelotas""},{""nome"":""Avaí"",""abreviacao"":""AVA"",""apelido"":"""",""nome_fantasia"":""Avaí""},{""nome"":""Chapecoense"",""abreviacao"":""CHA"",""apelido"":"""",""nome_fantasia"":""Chapecoense""},{""nome"":""Figueirense"",""abreviacao"":""FIG"",""apelido"":"""",""nome_fantasia"":""Figueirense""},{""nome"":""Joinville"",""abreviacao"":""JEC"",""apelido"":"""",""nome_fantasia"":""Joinville""},{""nome"":""Londrina"",""abreviacao"":""LON"",""apelido"":"""",""nome_fantasia"":""Londrina""},{""nome"":""Operário-PR"",""abreviacao"":""OPE"",""apelido"":"""",""nome_fantasia"":""Operário-PR""},{""nome"":""América-MG"",""abreviacao"":""AME"",""apelido"":""Coelho"",""nome_fantasia"":""América-MG""},{""nome"":""Confiança"",""abreviacao"":""CON"",""apelido"":"""",""nome_fantasia"":""Confiança""},{""nome"":""CRB"",""abreviacao"":""CRB"",""apelido"":"""",""nome_fantasia"":""CRB""},{""nome"":""CSA"",""abreviacao"":""CSA"",""apelido"":"""",""nome_fantasia"":""CSA""},{""nome"":""ASAArapiraca"",""abreviacao"":""ASA"",""apelido"":"""",""nome_fantasia"":""ASAArapiraca""},{""nome"":""Náutico"",""abreviacao"":""NAU"",""apelido"":"""",""nome_fantasia"":""Náutico""},{""nome"":""SantaCruz"",""abreviacao"":""STC"",""apelido"":"""",""nome_fantasia"":""SantaCruz""},{""nome"":""Treze"",""abreviacao"":""TRZ"",""apelido"":"""",""nome_fantasia"":""Treze""},{""nome"":""Botafogo-PB"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-PB""},{""nome"":""Campinense"",""abreviacao"":""CAM"",""apelido"":"""",""nome_fantasia"":""Campinense""},{""nome"":""ABC"",""abreviacao"":""ABC"",""apelido"":"""",""nome_fantasia"":""ABC""},{""nome"":""América-RN"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""América-RN""},{""nome"":""Ceará"",""abreviacao"":""CEA"",""apelido"":"""",""nome_fantasia"":""Ceará""},{""nome"":""Fortaleza"",""abreviacao"":""FOR"",""apelido"":""LeãodoPici"",""nome_fantasia"":""Fortaleza""},{""nome"":""MotoClub"",""abreviacao"":""MOT"",""apelido"":"""",""nome_fantasia"":""MotoClub""},{""nome"":""SampaioCorrêa"",""abreviacao"":""SAM"",""apelido"":"""",""nome_fantasia"":""SampaioCorrêa""},{""nome"":""Remo"",""abreviacao"":""REM"",""apelido"":"""",""nome_fantasia"":""Remo""},{""nome"":""Atlético-GO"",""abreviacao"":""ACG"",""apelido"":"""",""nome_fantasia"":""Atlético-GO""},{""nome"":""VilaNova"",""abreviacao"":""VIL"",""apelido"":"""",""nome_fantasia"":""VilaNova""},{""nome"":""SãoCaetano"",""abreviacao"":""SAO"",""apelido"":"""",""nome_fantasia"":""SãoCaetano""},{""nome"":""Brasiliense"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""Brasiliense""}]";

        public AuxiliarController(
            ApplicationContext applicationContext,
            ITimeService timeService,
            IMaterialService materialService,
            IFornecedorService fornecedorService,
            IContratoService contratoService)
        {
            _applicationContext = applicationContext;
            _timeService = timeService;
            _materialService = materialService;
            _fornecedorService = fornecedorService;
            _contratoService = contratoService;
        }

        [HttpGet("aux/teams/real/new")]
        public IActionResult GetRealTeams([FromQuery] FilterDto filterDto)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var teamList = JsonConvert.DeserializeObject<List<TeamDto>>(teamsJson, jsonSerializerSettings);

            teamList?.Shuffle();
            teamList = teamList?.Take(filterDto.Quantidade).ToList();

            return Ok(teamList);
        }

        [HttpGet("aux/teams/fake/new")]
        public async Task<IActionResult> GetFakeTeams([FromQuery] FilterDto filterDto)
        {
            var teams = await _timeService.GetNewFakes(filterDto.Quantidade);
            return Ok(teams);
        }

        [HttpGet("aux/materials/new")]
        public async Task<IActionResult> GetMaterials([FromQuery] FilterDto filterDto)
        {
            var materials = await _materialService.GetNewFakes(filterDto.Quantidade);
            return Ok(materials);
        }

        [HttpGet("aux/suppliers/new")]
        public async Task<IActionResult> GetSuppliers([FromQuery] FilterDto filterDto)
        {
            var suppliers = await _contratoService.GetNewFakes(filterDto.Quantidade); 
            return Ok(suppliers);
        }

        [HttpGet("aux/contracts/new")]
        public async Task<IActionResult> GetContracts([FromQuery] FilterDto filterDto)
        {
            var contracts = await _contratoService.GetNewFakes(filterDto.Quantidade);

            if(!contracts.Any())
                return BadRequest("Você precisa ter dados nas outras tabelas");

            return Ok(contracts);
        }

        [HttpGet("aux/teams/mix")]
        public async Task<IActionResult> GetMixTeams([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            return Ok(await _timeService.GetMix(filterDto.Quantidade, withChanges));
        }

        [HttpGet("aux/materials/mix")]
        public async Task<IActionResult> GetMixMaterials([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            return Ok(await _materialService.GetMix(filterDto.Quantidade, withChanges));
        }

        [HttpGet("aux/suppliers/mix")]
        public async Task<IActionResult> GetMixSuppliers([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            return Ok(await _fornecedorService.GetMix(filterDto.Quantidade, withChanges));
        }

        [HttpGet("aux/contracts/mix")]
        public async Task<IActionResult> GetMixContracts([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            var retult = await _contratoService.GetMix(filterDto.Quantidade, withChanges);
            
            if(retult is null)
                return BadRequest("Você precisa ter dados nas outras tabelas");

            return Ok(retult);
        }
    }

    public class FilterDto
    {
        public int Quantidade { get; set; } = 500;
    }
}