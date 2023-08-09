using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;

public class ConfigureTestes
{
    private List<MaterialDto> _materials;
    private List<TeamDto> _times;
    private List<FornecedorDto> _fornecedores;

    private ITimeService _timeService;
    private IMaterialService _materialService;
    private IFornecedorService _fornecedorService;
    private IContratoService _contratoService;

    public ConfigureTestes(
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

    public async Task CleanTables()
    {
        await _contratoService.CleanTable();
        await _materialService.CleanTable();
        await _timeService.CleanTable();
        await _fornecedorService.CleanTable();
    }

    public async Task BuildPayloads(int qtd)
    {
        _materials = await _materialService.GetNewFakes(200);
        _times = await _timeService.GetNewFakes(200);
        _fornecedores = await _fornecedorService.GetNewFakes(200);
    }

    public async Task RunInsertOneByOne()
    {
        //Insert Clube
        await _timeService.InsertRange(_times);

        //Insert Material
        await _materialService.InsertRange(_materials);

        //Insert Fornecedor
        await _fornecedorService.InsertRange(_fornecedores);

        //Insert Contrato
        await _contratoService.InsertRange(await _contratoService.GetNewFakes(100));
    }

    public async Task RunInsertBulk(int qtdContratos = 200)
    {
        //Insert Clube
        await _timeService.Upsert(_times);

        //Insert Material
        await _materialService.Upsert(_materials);

        //Insert Fornecedor
        await _fornecedorService.Upsert(_fornecedores);

        //Insert Contrato
        await _contratoService.Upsert(await _contratoService.GetNewFakes(qtdContratos));
    }
}