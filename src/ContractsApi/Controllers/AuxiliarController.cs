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
        public readonly string teamsJson = @"[{""nome"":""Outros"",""abreviacao"":""OUT"",""apelido"":"""",""nome_fantasia"":""Outros""},{""nome"":""Ipatinga"",""abreviacao"":""IPA"",""apelido"":"""",""nome_fantasia"":""Ipatinga""},{""nome"":""Cuiab�"",""abreviacao"":""CUI"",""apelido"":""Dourado"",""nome_fantasia"":""Cuiab�""},{""nome"":""Icasa"",""abreviacao"":""ICA"",""apelido"":"""",""nome_fantasia"":""Icasa""},{""nome"":""Oeste"",""abreviacao"":""OES"",""apelido"":"""",""nome_fantasia"":""Oeste""},{""nome"":""DuquedeCaxias"",""abreviacao"":""DUQ"",""apelido"":"""",""nome_fantasia"":""DuquedeCaxias""},{""nome"":""Americana"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Americana""},{""nome"":""Gr�mioPrudente"",""abreviacao"":""PRU"",""apelido"":"""",""nome_fantasia"":""Gr�mioPrudente""},{""nome"":""Luverdense"",""abreviacao"":""LUV"",""apelido"":"""",""nome_fantasia"":""Luverdense""},{""nome"":""Flamengo"",""abreviacao"":""FLA"",""apelido"":""Meng�o"",""nome_fantasia"":""Flamengo""},{""nome"":""Botafogo"",""abreviacao"":""BOT"",""apelido"":""Glorioso"",""nome_fantasia"":""Botafogo""},{""nome"":""Corinthians"",""abreviacao"":""COR"",""apelido"":""Tim�o"",""nome_fantasia"":""Corinthians""},{""nome"":""Bahia"",""abreviacao"":""BAH"",""apelido"":""TricolordeA�o"",""nome_fantasia"":""Bahia""},{""nome"":""Fluminense"",""abreviacao"":""FLU"",""apelido"":""Tricolor"",""nome_fantasia"":""Fluminense""},{""nome"":""Vasco"",""abreviacao"":""VAS"",""apelido"":""GigantedaColina"",""nome_fantasia"":""Vasco""},{""nome"":""Palmeiras"",""abreviacao"":""PAL"",""apelido"":""Verd�o"",""nome_fantasia"":""Palmeiras""},{""nome"":""S�oPaulo"",""abreviacao"":""SAO"",""apelido"":""Tricolor"",""nome_fantasia"":""S�oPaulo""},{""nome"":""Santos"",""abreviacao"":""SAN"",""apelido"":""Peixe"",""nome_fantasia"":""Santos""},{""nome"":""Portuguesa"",""abreviacao"":""POR"",""apelido"":"""",""nome_fantasia"":""Portuguesa""},{""nome"":""Guarani"",""abreviacao"":""GUA"",""apelido"":"""",""nome_fantasia"":""Guarani""},{""nome"":""Bragantino"",""abreviacao"":""BGT"",""apelido"":""MassaBruta"",""nome_fantasia"":""Bragantino""},{""nome"":""Atl�tico-MG"",""abreviacao"":""CAM"",""apelido"":""Galo"",""nome_fantasia"":""Atl�tico-MG""},{""nome"":""Cruzeiro"",""abreviacao"":""CRU"",""apelido"":""Raposa"",""nome_fantasia"":""Cruzeiro""},{""nome"":""Gr�mio"",""abreviacao"":""GRE"",""apelido"":""Imortal"",""nome_fantasia"":""Gr�mio""},{""nome"":""Internacional"",""abreviacao"":""INT"",""apelido"":""Colorado"",""nome_fantasia"":""Internacional""},{""nome"":""Juventude"",""abreviacao"":""JUV"",""apelido"":"""",""nome_fantasia"":""Juventude""},{""nome"":""Vit�ria"",""abreviacao"":""VIT"",""apelido"":"""",""nome_fantasia"":""Vit�ria""},{""nome"":""Crici�ma"",""abreviacao"":""CRI"",""apelido"":"""",""nome_fantasia"":""Crici�ma""},{""nome"":""Paran�"",""abreviacao"":""PAR"",""apelido"":"""",""nome_fantasia"":""Paran�""},{""nome"":""Goi�s"",""abreviacao"":""GOI"",""apelido"":""Esmeraldino"",""nome_fantasia"":""Goi�s""},{""nome"":""Paysandu"",""abreviacao"":""PAY"",""apelido"":"""",""nome_fantasia"":""Paysandu""},{""nome"":""Sport"",""abreviacao"":""SPT"",""apelido"":"""",""nome_fantasia"":""Sport""},{""nome"":""Athl�tico-PR"",""abreviacao"":""CAP"",""apelido"":""Furac�o"",""nome_fantasia"":""Athl�tico-PR""},{""nome"":""Coritiba"",""abreviacao"":""CFC"",""apelido"":""Coxa"",""nome_fantasia"":""Coritiba""},{""nome"":""Botafogo-SP"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-SP""},{""nome"":""PontePreta"",""abreviacao"":""PON"",""apelido"":"""",""nome_fantasia"":""PontePreta""},{""nome"":""BoaEsporte"",""abreviacao"":""BEC"",""apelido"":"""",""nome_fantasia"":""BoaEsporte""},{""nome"":""SantoAndr�"",""abreviacao"":""SAN"",""apelido"":"""",""nome_fantasia"":""SantoAndr�""},{""nome"":""S�oBento"",""abreviacao"":""SBE"",""apelido"":"""",""nome_fantasia"":""S�oBento""},{""nome"":""BrasildePelotas"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""BrasildePelotas""},{""nome"":""Ava�"",""abreviacao"":""AVA"",""apelido"":"""",""nome_fantasia"":""Ava�""},{""nome"":""Chapecoense"",""abreviacao"":""CHA"",""apelido"":"""",""nome_fantasia"":""Chapecoense""},{""nome"":""Figueirense"",""abreviacao"":""FIG"",""apelido"":"""",""nome_fantasia"":""Figueirense""},{""nome"":""Joinville"",""abreviacao"":""JEC"",""apelido"":"""",""nome_fantasia"":""Joinville""},{""nome"":""Londrina"",""abreviacao"":""LON"",""apelido"":"""",""nome_fantasia"":""Londrina""},{""nome"":""Oper�rio-PR"",""abreviacao"":""OPE"",""apelido"":"""",""nome_fantasia"":""Oper�rio-PR""},{""nome"":""Am�rica-MG"",""abreviacao"":""AME"",""apelido"":""Coelho"",""nome_fantasia"":""Am�rica-MG""},{""nome"":""Confian�a"",""abreviacao"":""CON"",""apelido"":"""",""nome_fantasia"":""Confian�a""},{""nome"":""CRB"",""abreviacao"":""CRB"",""apelido"":"""",""nome_fantasia"":""CRB""},{""nome"":""CSA"",""abreviacao"":""CSA"",""apelido"":"""",""nome_fantasia"":""CSA""},{""nome"":""ASAArapiraca"",""abreviacao"":""ASA"",""apelido"":"""",""nome_fantasia"":""ASAArapiraca""},{""nome"":""N�utico"",""abreviacao"":""NAU"",""apelido"":"""",""nome_fantasia"":""N�utico""},{""nome"":""SantaCruz"",""abreviacao"":""STC"",""apelido"":"""",""nome_fantasia"":""SantaCruz""},{""nome"":""Treze"",""abreviacao"":""TRZ"",""apelido"":"""",""nome_fantasia"":""Treze""},{""nome"":""Botafogo-PB"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-PB""},{""nome"":""Campinense"",""abreviacao"":""CAM"",""apelido"":"""",""nome_fantasia"":""Campinense""},{""nome"":""ABC"",""abreviacao"":""ABC"",""apelido"":"""",""nome_fantasia"":""ABC""},{""nome"":""Am�rica-RN"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Am�rica-RN""},{""nome"":""Cear�"",""abreviacao"":""CEA"",""apelido"":"""",""nome_fantasia"":""Cear�""},{""nome"":""Fortaleza"",""abreviacao"":""FOR"",""apelido"":""Le�odoPici"",""nome_fantasia"":""Fortaleza""},{""nome"":""MotoClub"",""abreviacao"":""MOT"",""apelido"":"""",""nome_fantasia"":""MotoClub""},{""nome"":""SampaioCorr�a"",""abreviacao"":""SAM"",""apelido"":"""",""nome_fantasia"":""SampaioCorr�a""},{""nome"":""Remo"",""abreviacao"":""REM"",""apelido"":"""",""nome_fantasia"":""Remo""},{""nome"":""Atl�tico-GO"",""abreviacao"":""ACG"",""apelido"":"""",""nome_fantasia"":""Atl�tico-GO""},{""nome"":""VilaNova"",""abreviacao"":""VIL"",""apelido"":"""",""nome_fantasia"":""VilaNova""},{""nome"":""S�oCaetano"",""abreviacao"":""SAO"",""apelido"":"""",""nome_fantasia"":""S�oCaetano""},{""nome"":""Brasiliense"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""Brasiliense""}]";

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
                return BadRequest("Voc� precisa ter dados nas outras tabelas");

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
                return BadRequest("Voc� precisa ter dados nas outras tabelas");

            return Ok(retult);
        }
    }

    public class FilterDto
    {
        public int Quantidade { get; set; } = 500;
    }
}