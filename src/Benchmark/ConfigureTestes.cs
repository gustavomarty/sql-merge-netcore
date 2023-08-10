using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;

public class ConfigureTestes
{
    private List<MaterialDto> _materials;
    private List<ClubeDto> _clubes;
    private List<FornecedorDto> _fornecedores;

    private IClubeService _clubeService;
    private IMaterialService _materialService;
    private IFornecedorService _fornecedorService;
    private IContratoService _contratoService;

    public ConfigureTestes(
        IClubeService clubeService,
        IMaterialService materialService,
        IFornecedorService fornecedorService,
        IContratoService contratoService)
    {
        _clubeService = clubeService;
        _materialService = materialService;
        _fornecedorService = fornecedorService;
        _contratoService = contratoService;
    }

    public async Task CleanTables()
    {
        await _contratoService.CleanTable();
        await _materialService.CleanTable();
        await _clubeService.CleanTable();
        await _fornecedorService.CleanTable();
    }

    public async Task BuildPayloads(int qtd)
    {
        _materials = await _materialService.GetNewFakes(qtd);
        _clubes = await _clubeService.GetNewFakes(qtd);
        _fornecedores = await _fornecedorService.GetNewFakes(qtd);
    }

    public async Task RunInsertBulk(int qtdContratos = 200)
    {
        //Insert Clube
        await _clubeService.Upsert(_clubes);

        //Insert Material
        await _materialService.Upsert(_materials);

        //Insert Fornecedor
        await _fornecedorService.Upsert(_fornecedores);

        //Insert Contrato
        await _contratoService.Upsert(await _contratoService.GetNewFakes(qtdContratos));
    }
}