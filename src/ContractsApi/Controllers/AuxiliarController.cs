using Bogus;
using Bogus.Extensions.Brazil;
using ContractsApi.Data;
using ContractsApi.Data.Entities;
using ContractsApi.Extensions;
using ContractsApi.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Contracts;

namespace ContractsApi.Controllers
{
    [ApiController]
    public class AuxiliarController : ControllerBase
    {
        public readonly string teamsJson = @"[{""nome"":""Outros"",""abreviacao"":""OUT"",""apelido"":"""",""nome_fantasia"":""Outros""},{""nome"":""Ipatinga"",""abreviacao"":""IPA"",""apelido"":"""",""nome_fantasia"":""Ipatinga""},{""nome"":""Cuiabá"",""abreviacao"":""CUI"",""apelido"":""Dourado"",""nome_fantasia"":""Cuiabá""},{""nome"":""Icasa"",""abreviacao"":""ICA"",""apelido"":"""",""nome_fantasia"":""Icasa""},{""nome"":""Oeste"",""abreviacao"":""OES"",""apelido"":"""",""nome_fantasia"":""Oeste""},{""nome"":""DuquedeCaxias"",""abreviacao"":""DUQ"",""apelido"":"""",""nome_fantasia"":""DuquedeCaxias""},{""nome"":""Americana"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""Americana""},{""nome"":""GrêmioPrudente"",""abreviacao"":""PRU"",""apelido"":"""",""nome_fantasia"":""GrêmioPrudente""},{""nome"":""Luverdense"",""abreviacao"":""LUV"",""apelido"":"""",""nome_fantasia"":""Luverdense""},{""nome"":""Flamengo"",""abreviacao"":""FLA"",""apelido"":""Mengão"",""nome_fantasia"":""Flamengo""},{""nome"":""Botafogo"",""abreviacao"":""BOT"",""apelido"":""Glorioso"",""nome_fantasia"":""Botafogo""},{""nome"":""Corinthians"",""abreviacao"":""COR"",""apelido"":""Timão"",""nome_fantasia"":""Corinthians""},{""nome"":""Bahia"",""abreviacao"":""BAH"",""apelido"":""TricolordeAço"",""nome_fantasia"":""Bahia""},{""nome"":""Fluminense"",""abreviacao"":""FLU"",""apelido"":""Tricolor"",""nome_fantasia"":""Fluminense""},{""nome"":""Vasco"",""abreviacao"":""VAS"",""apelido"":""GigantedaColina"",""nome_fantasia"":""Vasco""},{""nome"":""Palmeiras"",""abreviacao"":""PAL"",""apelido"":""Verdão"",""nome_fantasia"":""Palmeiras""},{""nome"":""SãoPaulo"",""abreviacao"":""SAO"",""apelido"":""Tricolor"",""nome_fantasia"":""SãoPaulo""},{""nome"":""Santos"",""abreviacao"":""SAN"",""apelido"":""Peixe"",""nome_fantasia"":""Santos""},{""nome"":""Portuguesa"",""abreviacao"":""POR"",""apelido"":"""",""nome_fantasia"":""Portuguesa""},{""nome"":""Guarani"",""abreviacao"":""GUA"",""apelido"":"""",""nome_fantasia"":""Guarani""},{""nome"":""Bragantino"",""abreviacao"":""BGT"",""apelido"":""MassaBruta"",""nome_fantasia"":""Bragantino""},{""nome"":""Atlético-MG"",""abreviacao"":""CAM"",""apelido"":""Galo"",""nome_fantasia"":""Atlético-MG""},{""nome"":""Cruzeiro"",""abreviacao"":""CRU"",""apelido"":""Raposa"",""nome_fantasia"":""Cruzeiro""},{""nome"":""Grêmio"",""abreviacao"":""GRE"",""apelido"":""Imortal"",""nome_fantasia"":""Grêmio""},{""nome"":""Internacional"",""abreviacao"":""INT"",""apelido"":""Colorado"",""nome_fantasia"":""Internacional""},{""nome"":""Juventude"",""abreviacao"":""JUV"",""apelido"":"""",""nome_fantasia"":""Juventude""},{""nome"":""Vitória"",""abreviacao"":""VIT"",""apelido"":"""",""nome_fantasia"":""Vitória""},{""nome"":""Criciúma"",""abreviacao"":""CRI"",""apelido"":"""",""nome_fantasia"":""Criciúma""},{""nome"":""Paraná"",""abreviacao"":""PAR"",""apelido"":"""",""nome_fantasia"":""Paraná""},{""nome"":""Goiás"",""abreviacao"":""GOI"",""apelido"":""Esmeraldino"",""nome_fantasia"":""Goiás""},{""nome"":""Paysandu"",""abreviacao"":""PAY"",""apelido"":"""",""nome_fantasia"":""Paysandu""},{""nome"":""Sport"",""abreviacao"":""SPT"",""apelido"":"""",""nome_fantasia"":""Sport""},{""nome"":""Athlético-PR"",""abreviacao"":""CAP"",""apelido"":""Furacão"",""nome_fantasia"":""Athlético-PR""},{""nome"":""Coritiba"",""abreviacao"":""CFC"",""apelido"":""Coxa"",""nome_fantasia"":""Coritiba""},{""nome"":""Botafogo-SP"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-SP""},{""nome"":""PontePreta"",""abreviacao"":""PON"",""apelido"":"""",""nome_fantasia"":""PontePreta""},{""nome"":""BoaEsporte"",""abreviacao"":""BEC"",""apelido"":"""",""nome_fantasia"":""BoaEsporte""},{""nome"":""SantoAndré"",""abreviacao"":""SAN"",""apelido"":"""",""nome_fantasia"":""SantoAndré""},{""nome"":""SãoBento"",""abreviacao"":""SBE"",""apelido"":"""",""nome_fantasia"":""SãoBento""},{""nome"":""BrasildePelotas"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""BrasildePelotas""},{""nome"":""Avaí"",""abreviacao"":""AVA"",""apelido"":"""",""nome_fantasia"":""Avaí""},{""nome"":""Chapecoense"",""abreviacao"":""CHA"",""apelido"":"""",""nome_fantasia"":""Chapecoense""},{""nome"":""Figueirense"",""abreviacao"":""FIG"",""apelido"":"""",""nome_fantasia"":""Figueirense""},{""nome"":""Joinville"",""abreviacao"":""JEC"",""apelido"":"""",""nome_fantasia"":""Joinville""},{""nome"":""Londrina"",""abreviacao"":""LON"",""apelido"":"""",""nome_fantasia"":""Londrina""},{""nome"":""Operário-PR"",""abreviacao"":""OPE"",""apelido"":"""",""nome_fantasia"":""Operário-PR""},{""nome"":""América-MG"",""abreviacao"":""AME"",""apelido"":""Coelho"",""nome_fantasia"":""América-MG""},{""nome"":""Confiança"",""abreviacao"":""CON"",""apelido"":"""",""nome_fantasia"":""Confiança""},{""nome"":""CRB"",""abreviacao"":""CRB"",""apelido"":"""",""nome_fantasia"":""CRB""},{""nome"":""CSA"",""abreviacao"":""CSA"",""apelido"":"""",""nome_fantasia"":""CSA""},{""nome"":""ASAArapiraca"",""abreviacao"":""ASA"",""apelido"":"""",""nome_fantasia"":""ASAArapiraca""},{""nome"":""Náutico"",""abreviacao"":""NAU"",""apelido"":"""",""nome_fantasia"":""Náutico""},{""nome"":""SantaCruz"",""abreviacao"":""STC"",""apelido"":"""",""nome_fantasia"":""SantaCruz""},{""nome"":""Treze"",""abreviacao"":""TRZ"",""apelido"":"""",""nome_fantasia"":""Treze""},{""nome"":""Botafogo-PB"",""abreviacao"":""BOT"",""apelido"":"""",""nome_fantasia"":""Botafogo-PB""},{""nome"":""Campinense"",""abreviacao"":""CAM"",""apelido"":"""",""nome_fantasia"":""Campinense""},{""nome"":""ABC"",""abreviacao"":""ABC"",""apelido"":"""",""nome_fantasia"":""ABC""},{""nome"":""América-RN"",""abreviacao"":""AME"",""apelido"":"""",""nome_fantasia"":""América-RN""},{""nome"":""Ceará"",""abreviacao"":""CEA"",""apelido"":"""",""nome_fantasia"":""Ceará""},{""nome"":""Fortaleza"",""abreviacao"":""FOR"",""apelido"":""LeãodoPici"",""nome_fantasia"":""Fortaleza""},{""nome"":""MotoClub"",""abreviacao"":""MOT"",""apelido"":"""",""nome_fantasia"":""MotoClub""},{""nome"":""SampaioCorrêa"",""abreviacao"":""SAM"",""apelido"":"""",""nome_fantasia"":""SampaioCorrêa""},{""nome"":""Remo"",""abreviacao"":""REM"",""apelido"":"""",""nome_fantasia"":""Remo""},{""nome"":""Atlético-GO"",""abreviacao"":""ACG"",""apelido"":"""",""nome_fantasia"":""Atlético-GO""},{""nome"":""VilaNova"",""abreviacao"":""VIL"",""apelido"":"""",""nome_fantasia"":""VilaNova""},{""nome"":""SãoCaetano"",""abreviacao"":""SAO"",""apelido"":"""",""nome_fantasia"":""SãoCaetano""},{""nome"":""Brasiliense"",""abreviacao"":""BRA"",""apelido"":"""",""nome_fantasia"":""Brasiliense""}]";

        private readonly ApplicationContext _applicationContext;

        public AuxiliarController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
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
        public IActionResult GetFakeTeams([FromQuery] FilterDto filterDto)
        {
            var teams = GetNewFakeTeams(filterDto.Quantidade);
            return Ok(teams);
        }

        [HttpGet("aux/materials/new")]
        public IActionResult GetMaterials([FromQuery] FilterDto filterDto)
        {
            var materials = GetNewFakeMaterials(filterDto.Quantidade);
            return Ok(materials);
        }

        [HttpGet("aux/suppliers/new")]
        public IActionResult GetSuppliers([FromQuery] FilterDto filterDto)
        {
            var suppliers = GetNewFakeSuppliers(filterDto.Quantidade);
            return Ok(suppliers);
        }

        [HttpGet("aux/contracts/new")]
        public async Task<IActionResult> GetContracts([FromQuery] FilterDto filterDto)
        {
            var contracts = await GetNewFakeContracts(filterDto.Quantidade);

            if(!contracts.Any())
                return BadRequest("Você precisa ter dados nas outras tabelas");

            return Ok(contracts);
        }

        [HttpGet("aux/teams/mix")]
        public async Task<IActionResult> GetMixTeams([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            var existingTeams = await _applicationContext.Set<Clube>().ToListAsync();

            var quantityForGenerateTeams = filterDto.Quantidade / 2;
            var quantityForExistingTeams = filterDto.Quantidade / 2;

            if(existingTeams.Count < quantityForGenerateTeams)
            {
                quantityForExistingTeams = existingTeams.Count;
                quantityForGenerateTeams += (quantityForGenerateTeams - existingTeams.Count);
            }

            var newTeams = GetNewFakeTeams(quantityForGenerateTeams);
            existingTeams.Shuffle();

            existingTeams = existingTeams.Take(quantityForExistingTeams).ToList();


            List<TeamDto> result = new();
            result.AddRange(newTeams);
            result.AddRange(existingTeams.Select(x => {
                
                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new TeamDto
                {
                    Nome = x.Nome,
                    Abreviacao = x.Abreviacao,
                    Apelido = changeItems ? $"ALTERADO: {x.Apelido}" : x.Apelido ?? string.Empty
                };
            }));

            return Ok(result);
        }

        [HttpGet("aux/materials/mix")]
        public async Task<IActionResult> GetMixMaterials([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            var existingMaterials = await _applicationContext.Set<Material>().ToListAsync();

            var quantityForGenerateMaterials = filterDto.Quantidade / 2;
            var quantityForExistingMaterials = filterDto.Quantidade / 2;

            if(existingMaterials.Count < quantityForGenerateMaterials)
            {
                quantityForExistingMaterials = existingMaterials.Count;
                quantityForGenerateMaterials += (quantityForGenerateMaterials - existingMaterials.Count);
            }

            var newMaterials = GetNewFakeMaterials(quantityForGenerateMaterials);
            existingMaterials.Shuffle();

            existingMaterials = existingMaterials.Take(quantityForExistingMaterials).ToList();


            List<MaterialDto> result = new();
            result.AddRange(newMaterials);
            result.AddRange(existingMaterials.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new MaterialDto
                {
                    Nome = changeItems ? $"ALTERADO: {x.Nome}" : x.Nome,
                    Numero = x.Numero
                };
            }));

            return Ok(result);
        }

        [HttpGet("aux/suppliers/mix")]
        public async Task<IActionResult> GetMixSuppliers([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            var existingData = await _applicationContext.Set<Fornecedor>().ToListAsync();

            var quantityForGenerateData = filterDto.Quantidade / 2;
            var quantityForExistingData = filterDto.Quantidade / 2;

            if(existingData.Count < quantityForGenerateData)
            {
                quantityForExistingData = existingData.Count;
                quantityForGenerateData += (quantityForGenerateData - existingData.Count);
            }

            var newData = GetNewFakeSuppliers(quantityForGenerateData);
            existingData.Shuffle();

            existingData = existingData.Take(quantityForExistingData).ToList();


            List<FornecedorDto> result = new();
            result.AddRange(newData);
            result.AddRange(existingData.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new FornecedorDto
                {
                    Nome = changeItems ? $"ALTERADO: {x.Nome}" : x.Nome,
                    Documento = x.Documento,
                    Cep = x.Cep
                };
            }));

            return Ok(result);
        }

        [HttpGet("aux/contracts/mix")]
        public async Task<IActionResult> GetMixContracts([FromQuery] FilterDto filterDto, [FromQuery] bool withChanges)
        {
            var existingData = await _applicationContext.Set<Contrato>().ToListAsync();

            var quantityForGenerateData = filterDto.Quantidade / 2;
            var quantityForExistingData = filterDto.Quantidade / 2;

            if(existingData.Count < quantityForGenerateData)
            {
                quantityForExistingData = existingData.Count;
                quantityForGenerateData += (quantityForGenerateData - existingData.Count);
            }

            var newData = await GetNewFakeContracts(quantityForGenerateData);
            if(!newData.Any())
                return BadRequest("Você precisa ter dados nas outras tabelas");

                existingData.Shuffle();

            existingData = existingData.Take(quantityForExistingData).ToList();


            List<ContratoDto> result = new();
            result.AddRange(newData);
            result.AddRange(existingData.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new ContratoDto
                {
                    Descricao = changeItems ? $"ALTERADO: {x.Descricao}" : x.Descricao,
                    DocumentoFornecedor = x.Fornecedor.Documento,
                    NomeClube = x.Clube.Nome,
                    NumeroMaterial = x.Material.Numero,
                    Numero = x.Numero,
                    Inicio = x.Inicio,
                    Fim = x.Fim,
                    Preco = x.Preco
                };
            }));

            return Ok(result);
        }

        private static List<TeamDto> GetNewFakeTeams(int quantity)
        {
            var faker = new Faker<TeamDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Abreviacao, f => f.Company.CompanyName()[..3].ToUpper())
                .RuleFor(x => x.Apelido, string.Empty);

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Nome)
                .Select(g => g.First());

            return responseGroup.ToList();
        }

        private static List<MaterialDto> GetNewFakeMaterials(int quantity)
        {
            var faker = new Faker<MaterialDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Commerce.ProductName())
                .RuleFor(x => x.Numero, f => f.Random.Number(0, 99999).ToString().PadLeft(5, '0'));

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Numero)
                .Select(g => g.First());

            return responseGroup.ToList();
        }

        private static List<FornecedorDto> GetNewFakeSuppliers(int quantity)
        {
            var faker = new Faker<FornecedorDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Documento, f => f.Company.Cnpj(false))
                .RuleFor(x => x.Cep, f => f.Random.Number(10000000, 99999999).ToString());

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Documento)
                .Select(g => g.First());

            return responseGroup.ToList();
        }

        private async Task<List<ContratoDto>> GetNewFakeContracts(int quantity)
        {
            var teamNames = await _applicationContext.Set<Clube>().Select(x => x.Nome).ToListAsync();
            var materialNumbers = await _applicationContext.Set<Material>().Select(x => x.Numero).ToListAsync();
            var supplierDocuments = await _applicationContext.Set<Fornecedor>().Select(x => x.Documento).ToListAsync();

            if(!teamNames.Any() || !materialNumbers.Any() || !supplierDocuments.Any())
            {
                return new List<ContratoDto>();
            }

            var faker = new Faker<ContratoDto>("pt_BR")
                .RuleFor(x => x.NomeClube, f => f.PickRandom(teamNames))
                .RuleFor(x => x.NumeroMaterial, f => f.PickRandom(materialNumbers))
                .RuleFor(x => x.DocumentoFornecedor, f => f.PickRandom(supplierDocuments))
                .RuleFor(x => x.Numero, f => f.Random.Number(10000, 99999).ToString())
                .RuleFor(x => x.Preco, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(x => x.Inicio, f => f.Date.Recent(365))
                .RuleFor(x => x.Fim, f => f.Date.Future(10));

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => new { x.NomeClube, x.NumeroMaterial, x.DocumentoFornecedor, x.Numero })
                .Select(g => g.First());

            return responseGroup.ToList();
        }
    }

    public class FilterDto
    {
        public int Quantidade { get; set; } = 500;
    }
}