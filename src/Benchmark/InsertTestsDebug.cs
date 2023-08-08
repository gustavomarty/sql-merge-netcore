using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;

public class InsertTestsDebug
{
    private List<MaterialDto> _materials;
    private List<TeamDto> _times;
    private List<FornecedorDto> _fornecedores;

    private ITimeService _timeService;
    private IMaterialService _materialService;
    private IFornecedorService _fornecedorService;
    private IContratoService _contratoService;

    public InsertTestsDebug(
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

    public async Task BuildPayloads()
    {
        _materials = await _materialService.GetNewFakes(100);
        _times = await _timeService.GetNewFakes(100);
        _fornecedores = await _fornecedorService.GetNewFakes(100);
    }

    public async Task RunInsertOneByOne()
    {
        //Insert Clube
        await _timeService.Create(_times);

        //Insert Material
        await _materialService.Create(_materials);

        //Insert Fornecedor
        await _fornecedorService.Create(_fornecedores);

        //Insert Contrato
        await _contratoService.Create(await _contratoService.GetNewFakes(100));

        //Limpa tabelas
        await _timeService.CleanTable();
        await _materialService.CleanTable();
        await _fornecedorService.CleanTable();
        await _contratoService.CleanTable();
    }

    public async Task RunInsertBulk()
    {
        //Insert Clube
        await _timeService.CreateBulk(_times);

        //Insert Material
        await _materialService.CreateBulk(_materials);

        //Insert Fornecedor
        await _fornecedorService.CreateBulk(_fornecedores);

        //Insert Contrato
        await _contratoService.CreateBulk(await _contratoService.GetNewFakes(100));

        //Limpa tabelas
        await _timeService.CleanTable();
        await _materialService.CleanTable();
        await _fornecedorService.CleanTable();
        await _contratoService.CleanTable();
    }
}