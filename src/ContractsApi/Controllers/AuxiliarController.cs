using Bogus;
using Bogus.Extensions.Brazil;
using ContractsApi.Data;
using ContractsApi.Data.Entities;
using ContractsApi.Extensions;
using ContractsApi.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ContractsApi.Controllers
{
    [ApiController]
    public class AuxiliarController : ControllerBase
    {
        public readonly string teamsJson = @"[{""nome"":""Outros"",""abreviacao"":""OUT"",""apelido"":"""",""nome_fantasia"":""Outros""},{""nome"":""Ipatinga"",""abreviacao"":""IPA"",""apelido"":"""",""nome_fantasia"":""Ipatinga""},{""nome"":""Cuiab�"",""abreviacao"":""CUI"",""apelido"":""Dourado"",""nome_fantasia"":""Cuiab�""},{""nome"":""Icasa"",""abreviacao"":""ICA"",""apelido"":"""",""nome_fantasia"":""Icasa""},{""nome"":""Oeste"",""abreviacao"":""OES"",""apelido"":"""",""nome_fantasia"":""Oeste""},{""nome"":""DuquedeCaxias"",""abreviacao"":""DUQ"",""apelido"":"""",""nome_fantasia"":""DuquedeCaxias""},{""nome"":""Americana"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Americana""},{""nome"":""Gr�mioPrudente"",""abreviacao"":""PRU"",""apelido"":"""",""nome_fantasia"":""Gr�mioPrudente""},{""nome"":""Luverdense"",""abreviacao"":""LUV"",""apelido"":"""",""nome_fantasia"":""Luverdense""},{""nome"":""Flamengo"",""abreviacao"":""FLA"",""apelido"":""Meng�o"",""nome_fantasia"":""Flamengo""},{""nome"":""Botafogo"",""abreviacao"":""BOT"",""apelido"":""Glorioso"",""nome_fantasia"":""Botafogo""},{""nome"":""Corinthians"",""abreviacao"":""COR"",""apelido"":""Tim�o"",""nome_fantasia"":""Corinthians""},{""nome"":""Bahia"",""abreviacao"":""BAH"",""apelido"":""TricolordeA�o"",""nome_fantasia"":""Bahia""},{""nome"":""Fluminense"",""abreviacao"":""FLU"",""apelido"":""Tricolor"",""nome_fantasia"":""Fluminense""},{""nome"":""Vasco"",""abreviacao"":""VAS"",""apelido"":""GigantedaColina"",""nome_fantasia"":""Vasco""},{""nome"":""Palmeiras"",""abreviacao"":""PAL"",""apelido"":""Verd�o"",""nome_fantasia"":""Palmeiras""},{""nome"":""S�oPaulo"",""abreviacao"":""SAO"",""apelido"":""Tricolor"",""nome_fantasia"":""S�oPaulo""},{""nome"":""Santos"",""abreviacao"":""SAN"",""apelido"":""Peixe"",""nome_fantasia"":""Santos""},{""nome"":""Portuguesa"",""abreviacao"":""POR"",""apelido"":"""",""nome_fantasia"":""Portuguesa""},{""nome"":""Guarani"",""abreviacao"":""GUA"",""apelido"":"""",""nome_fantasia"":""Guarani""},{""nome"":""Bragantino"",""abreviacao"":""BGT"",""apelido"":""MassaBruta"",""nome_fantasia"":""Bragantino""},{""nome"":""Atl�tico-MG"",""abreviacao"":""CAM"",""apelido"":""Galo"",""nome_fantasia"":""Atl�tico-MG""},{""nome"":""Cruzeiro"",""abreviacao"":""CRU"",""apelido"":""Raposa"",""nome_fantasia"":""Cruzeiro""},{""nome"":""Gr�mio"",""abreviacao"":""GRE"",""apelido"":""Imortal"",""nome_fantasia"":""Gr�mio""},{""nome"":""Internacional"",""abreviacao"":""INT"",""apelido"":""Colorado"",""nome_fantasia"":""Internacional""},{""nome"":""Juventude"",""abreviacao"":""JUV"",""apelido"":"""",""nome_fantasia"":""Juventude""},{""nome"":""Vit�ria"",""abreviacao"":""VIT"",""apelido"":"""",""nome_fantasia"":""Vit�ria""},{""nome"":""Crici�ma"",""abreviacao"":""CRI"",""apelido"":"""",""nome_fantasia"":""Crici�ma""},{""nome"":""Paran�"",""abreviacao"":""PAR"",""apelido"":"""",""nome_fantasia"":""Paran�""},{""nome"":""Goi�s"",""abreviacao"":""GOI"",""apelido"":""Esmeraldino"",""nome_fantasia"":""Goi�s""},{""nome"":""Paysandu"",""abreviacao"":""PAY"",""apelido"":"""",""nome_fantasia"":""Paysandu""},{""nome"":""Sport"",""abreviacao"":""SPT"",""apelido"":"""",""nome_fantasia"":""Sport""},{""nome"":""Athl�tico-PR"",""abreviacao"":""CAP"",""apelido"":""Furac�o"",""nome_fantasia"":""Athl�tico-PR""},{""nome"":""Coritiba"",""abreviacao"":""CFC"",""apelido"":""Coxa"",""nome_fantasia"":""Coritiba""},{""nome"":""Botafogo-SP"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-SP""},{""nome"":""PontePreta"",""abreviacao"":""PON"",""apelido"":"""",""nome_fantasia"":""PontePreta""},{""nome"":""BoaEsporte"",""abreviacao"":""BEC"",""apelido"":"""",""nome_fantasia"":""BoaEsporte""},{""nome"":""SantoAndr�"",""abreviacao"":""SAN"",""apelido"":"""",""nome_fantasia"":""SantoAndr�""},{""nome"":""S�oBento"",""abreviacao"":""SBE"",""apelido"":"""",""nome_fantasia"":""S�oBento""},{""nome"":""BrasildePelotas"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""BrasildePelotas""},{""nome"":""Ava�"",""abreviacao"":""AVA"",""apelido"":"""",""nome_fantasia"":""Ava�""},{""nome"":""Chapecoense"",""abreviacao"":""CHA"",""apelido"":"""",""nome_fantasia"":""Chapecoense""},{""nome"":""Figueirense"",""abreviacao"":""FIG"",""apelido"":"""",""nome_fantasia"":""Figueirense""},{""nome"":""Joinville"",""abreviacao"":""JEC"",""apelido"":"""",""nome_fantasia"":""Joinville""},{""nome"":""Londrina"",""abreviacao"":""LON"",""apelido"":"""",""nome_fantasia"":""Londrina""},{""nome"":""Oper�rio-PR"",""abreviacao"":""OPE"",""apelido"":"""",""nome_fantasia"":""Oper�rio-PR""},{""nome"":""Am�rica-MG"",""abreviacao"":""AME"",""apelido"":""Coelho"",""nome_fantasia"":""Am�rica-MG""},{""nome"":""Confian�a"",""abreviacao"":""CON"",""apelido"":"""",""nome_fantasia"":""Confian�a""},{""nome"":""CRB"",""abreviacao"":""CRB"",""apelido"":"""",""nome_fantasia"":""CRB""},{""nome"":""CSA"",""abreviacao"":""CSA"",""apelido"":"""",""nome_fantasia"":""CSA""},{""nome"":""ASAArapiraca"",""abreviacao"":""ASA"",""apelido"":"""",""nome_fantasia"":""ASAArapiraca""},{""nome"":""N�utico"",""abreviacao"":""NAU"",""apelido"":"""",""nome_fantasia"":""N�utico""},{""nome"":""SantaCruz"",""abreviacao"":""STC"",""apelido"":"""",""nome_fantasia"":""SantaCruz""},{""nome"":""Treze"",""abreviacao"":""TRZ"",""apelido"":"""",""nome_fantasia"":""Treze""},{""nome"":""Botafogo-PB"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-PB""},{""nome"":""Campinense"",""abreviacao"":""CAM"",""apelido"":"""",""nome_fantasia"":""Campinense""},{""nome"":""ABC"",""abreviacao"":""ABC"",""apelido"":"""",""nome_fantasia"":""ABC""},{""nome"":""Am�rica-RN"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Am�rica-RN""},{""nome"":""Cear�"",""abreviacao"":""CEA"",""apelido"":"""",""nome_fantasia"":""Cear�""},{""nome"":""Fortaleza"",""abreviacao"":""FOR"",""apelido"":""Le�odoPici"",""nome_fantasia"":""Fortaleza""},{""nome"":""MotoClub"",""abreviacao"":""MOT"",""apelido"":"""",""nome_fantasia"":""MotoClub""},{""nome"":""SampaioCorr�a"",""abreviacao"":""SAM"",""apelido"":"""",""nome_fantasia"":""SampaioCorr�a""},{""nome"":""Remo"",""abreviacao"":""REM"",""apelido"":"""",""nome_fantasia"":""Remo""},{""nome"":""Atl�tico-GO"",""abreviacao"":""ACG"",""apelido"":"""",""nome_fantasia"":""Atl�tico-GO""},{""nome"":""VilaNova"",""abreviacao"":""VIL"",""apelido"":"""",""nome_fantasia"":""VilaNova""},{""nome"":""S�oCaetano"",""abreviacao"":""SAO"",""apelido"":"""",""nome_fantasia"":""S�oCaetano""},{""nome"":""Brasiliense"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""Brasiliense""}]";

        private readonly ApplicationContext _applicationContext;

        public AuxiliarController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        [HttpGet("aux/teams/real")]
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

        [HttpGet("aux/teams/fake")]
        public IActionResult GetFakeTeams([FromQuery] FilterDto filterDto)
        {
            var faker = new Faker<TeamDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Abreviacao, f => f.Company.CompanyName()[..3].ToUpper())
                .RuleFor(x => x.Apelido, string.Empty);

            var response = faker.Generate(filterDto.Quantidade);

            var responseGroup = response.GroupBy(x => x.Nome)
                .Select(g => g.First());

            return Ok(responseGroup);
        }

        [HttpGet("aux/materials")]
        public IActionResult GetMaterials([FromQuery] FilterDto filterDto)
        {
            var faker = new Faker<MaterialDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Commerce.ProductName())
                .RuleFor(x => x.Numero, f => f.Random.Number(0, 99999).ToString().PadLeft(5, '0'));

            var response = faker.Generate(filterDto.Quantidade);

            var responseGroup = response.GroupBy(x => x.Numero)
                .Select(g => g.First());

            return Ok(responseGroup);
        }

        [HttpGet("aux/suppliers")]
        public IActionResult GetSuppliers([FromQuery] FilterDto filterDto)
        {
            var faker = new Faker<FornecedorDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Documento, f => f.Company.Cnpj(false))
                .RuleFor(x => x.Cep, f => f.Random.Number(10000000, 99999999).ToString());

            var response = faker.Generate(filterDto.Quantidade);

            var responseGroup = response.GroupBy(x => x.Documento)
                .Select(g => g.First());

            return Ok(responseGroup);
        }

        [HttpGet("aux/contracts")]
        public async Task<IActionResult> GetContracts([FromQuery] FilterDto filterDto)
        {
            var teamsIds = await _applicationContext.Set<Clube>().Select(x => x.Id).ToListAsync();
            var materialsIds = await _applicationContext.Set<Material>().Select(x => x.Id).ToListAsync();
            var supplierIds = await _applicationContext.Set<Fornecedor>().Select(x => x.Id).ToListAsync();

            if(!teamsIds.Any() || !materialsIds.Any() || !supplierIds.Any())
            {
                return BadRequest("Voc� precisa ter dados nas outras tabelas");
            }

            var faker = new Faker<ContratoDto>("pt_BR")
                .RuleFor(x => x.IdClube, f => f.PickRandom(teamsIds))
                .RuleFor(x => x.IdMaterial, f => f.PickRandom(materialsIds))
                .RuleFor(x => x.IdFornecedor, f => f.PickRandom(supplierIds))
                .RuleFor(x => x.Numero, f => f.Random.Number(10000, 99999).ToString())
                .RuleFor(x => x.Preco, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(x => x.Inicio, f => f.Date.Recent())
                .RuleFor(x => x.Inicio, f => f.Date.Future(10));

            var response = faker.Generate(filterDto.Quantidade);

            var responseGroup = response.GroupBy(x => new { x.IdClube, x.IdFornecedor, x.IdMaterial })
                .Select(g => g.First());

            return Ok(responseGroup);
        }
    }

    public class FilterDto
    {
        public int Quantidade { get; set; } = 500;
    }
}