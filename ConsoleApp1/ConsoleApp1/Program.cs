using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using ConsoleTables;
using ServiceReference1;

class Program
{
    static ReportingService2010SoapClient rsclient = null;

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        InitializeReportingServiceClient();

        await ListAllFoldersAndSubfoldersAsync("/");
        await ListAllReportsAsync("/");
        await ListAllDataSourcesAsync("/");



        //// Define o caminho da pasta onde os DataSources estão localizados
        //string folderPath = "/DataSources";

        //// Exemplo: Obter conteúdo de um DataSource
        //string dataSourcePath = $"{folderPath}/FonteTeste";
        //await GetAndPrintDataSourceContent(dataSourcePath);

        //// Testar conexão do DataSource
        //bool testResult = await TestDataSourceConnection(dataSourcePath, "admin", "senha@123");
        //Console.WriteLine($"Resultado do teste de conexão para {dataSourcePath}: {testResult}");

        //// Atualizar senha do DataSource
        //await UpdateDataSourcePasswordAsync(dataSourcePath, "senha@123");


        // await DownloadReportsAsync("/",@"c:/temp/");

        Console.WriteLine("Pressione qualquer tecla para sair.");
        Console.ReadKey();
    }

    private static void InitializeReportingServiceClient()
    {
        BasicHttpBinding rsBinding = new BasicHttpBinding();
        rsBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
        rsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

        EndpointAddress rsEndpointAddress = new EndpointAddress("http://desktop-hu97nsf/reportserver/ReportService2010.asmx");

        rsclient = new ReportingService2010SoapClient(rsBinding, rsEndpointAddress);
    }
    private static async System.Threading.Tasks.Task GetAndPrintDataSourceContent(string dataSourcePath)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter conteúdo do DataSource
            var getDataSourceResponse = await rsclient.GetDataSourceContentsAsync(trustedUserHeader, dataSourcePath);
            DataSourceDefinition dataSourceDefinition = getDataSourceResponse.Definition;

            // Imprimir detalhes do DataSource em formato de tabela
            Console.WriteLine($"DataSource Content for: {dataSourcePath}");
            //PrintTable("Property", "Value", dataSourceDefinition.ConnectString, dataSourceDefinition.UserName, dataSourceDefinition.Password);
            PrintTable(dataSourceDefinition);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting DataSource content: {ex.Message}");
        }
    }
    private static async Task<bool> TestDataSourceConnection(string dataSourcePath, string userName, string password)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter conteúdo do DataSource
            var getDataSourceResponse = await rsclient.GetDataSourceContentsAsync(trustedUserHeader, dataSourcePath);
            DataSourceDefinition dataSourceDefinition = getDataSourceResponse.Definition;
            dataSourceDefinition.Password = password;

            // Testar conexão do DataSource
            var testConnectResponse = await rsclient.TestConnectForDataSourceDefinitionAsync(
                new TestConnectForDataSourceDefinitionRequest
                {
                    TrustedUserHeader = trustedUserHeader,
                    UserName = userName,
                    Password = password,
                    DataSourceDefinition = dataSourceDefinition
                });

            // Imprimir resultados do teste em formato de tabela
            Console.WriteLine($"Connection Test Result for: {dataSourcePath}");
            PrintTable(testConnectResponse);

            return testConnectResponse.TestConnectForDataSourceDefinitionResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing DataSource connection: {ex.Message}");
            return false;
        }
    }
    private static async System.Threading.Tasks.Task UpdateDataSourcePasswordAsync(string dataSourcePath, string newPassword)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter conteúdo do DataSource
            var getDataSourceResponse = await rsclient.GetDataSourceContentsAsync(trustedUserHeader, dataSourcePath);
            DataSourceDefinition dataSourceDefinition = getDataSourceResponse.Definition;

            // Atualizar a senha no DataSourceDefinition
            dataSourceDefinition.Password = newPassword;

            // Definir o conteúdo atualizado do DataSource
            var setDataSourceResponse = await rsclient.SetDataSourceContentsAsync(trustedUserHeader, dataSourcePath, dataSourceDefinition);

            Console.WriteLine($"DataSource password updated successfully for {dataSourcePath}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating DataSource password: {ex.Message}");
        }
    }
    private static void PrintTable(object dataObject)
    {
        // Obter todas as propriedades públicas do objeto
        PropertyInfo[] properties = dataObject.GetType().GetProperties();

        // Criar a tabela usando o ConsoleTables
        var table = new ConsoleTable("Property", "Value");

        // Adicionar linhas à tabela com os valores das propriedades
        foreach (var property in properties)
        {
            string propertyName = property.Name;
            object propertyValue = property.GetValue(dataObject);

            // Adicionar uma linha à tabela com chave e valor
            table.AddRow(propertyName, propertyValue);
        }

        // Imprimir a tabela
        Console.WriteLine(table.ToString());
    }

    private static async System.Threading.Tasks.Task ListAllFoldersAndSubfoldersAsync(string folderPath)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter a lista de pastas e subpastas
            var listChildrenResponse = await rsclient.ListChildrenAsync(trustedUserHeader, folderPath, false);
            CatalogItem[] items = listChildrenResponse.CatalogItems;

            // Imprimir as pastas e subpastas
            Console.WriteLine($"Folders and Subfolders in: {folderPath}");
            foreach (var item in items)
            {

                if (item.TypeName == "Folder")
                {
                    Console.WriteLine($"Folder: {item.Name}");
                    Console.WriteLine($"TypeName: {item.TypeName}");

                    PrintTable(item);

                    await ListAllFoldersAndSubfoldersAsync(item.Path);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing folders and subfolders: {ex.Message}");
        }
    }
    private static async System.Threading.Tasks.Task ListAllReportsAsync(string folderPath)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter a lista de relatórios na pasta especificada
            var listChildrenResponse = await rsclient.ListChildrenAsync(trustedUserHeader, folderPath, true);
            CatalogItem[] items = listChildrenResponse.CatalogItems;

            // Imprimir os relatórios
            Console.WriteLine($"Reports in: {folderPath}");
            foreach (var item in items)
            {

                if (item.TypeName == "Report")
                {
                    PrintTable(item);
                    await DownloadReportAsync(item.Name,item.Path, @"C:\\temp\");
                    Console.WriteLine($"Report: {item.Name}");
                    Console.WriteLine($"TypeName: {item.TypeName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing reports: {ex.Message}");
        }
    }
    private static async System.Threading.Tasks.Task ListAllDataSourcesAsync(string folderPath)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter a lista de relatórios na pasta especificada
            var listChildrenResponse = await rsclient.ListChildrenAsync(trustedUserHeader, folderPath, true);
            CatalogItem[] items = listChildrenResponse.CatalogItems;

            // Imprimir os relatórios
            Console.WriteLine($"DataSources in: {folderPath}");
            foreach (var item in items)
            {

                if (item.TypeName == "DataSource")
                {
                    PrintTable(item);
                    await GetAndPrintDataSourceContent(item.Path);
                    Console.WriteLine($"DataSource: {item.Name}");
                    Console.WriteLine($"TypeName: {item.TypeName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing reports: {ex.Message}");
        }
    }
    private static async System.Threading.Tasks.Task DownloadReportsAsync(string folderPath, string downloadDirectory)
    {
        TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

        try
        {
            // Obter a lista de relatórios na pasta especificada
            var listChildrenResponse = await rsclient.ListChildrenAsync(trustedUserHeader, folderPath, true);
            CatalogItem[] items = listChildrenResponse.CatalogItems;

            // Baixar cada relatório para o diretório especificado
            foreach (var item in items)
            {
                //if (item.Type == ItemTypeEnum.Report)
                //{
                // Obter o conteúdo do relatório
                var getReportResponse = await rsclient.GetItemDefinitionAsync(trustedUserHeader, item.Path);
                byte[] reportContent = getReportResponse.Definition;

                // Salvar o relatório no diretório especificado
                string fileName = $"{downloadDirectory}\\{item.Name}.rdl";
                System.IO.File.WriteAllBytes(fileName, reportContent);
                Console.WriteLine($"Report downloaded to: {fileName}");
                // }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading reports: {ex.Message}");
        }
    }

    private static async System.Threading.Tasks.Task DownloadReportAsync(string name, string folderPath, string downloadDirectory)
    {
        try
        {
            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

            // Obter o conteúdo do relatório
            var getReportResponse = await rsclient.GetItemDefinitionAsync(trustedUserHeader, folderPath);
            byte[] reportContent = getReportResponse.Definition;

            // Construir o caminho completo do arquivo de destino
            string fileName = Path.Combine(downloadDirectory + folderPath, $"{name}.rdl");
            

            CreateDirectoriesFromUrl(downloadDirectory + folderPath);

            // Salvar o relatório no diretório especificado
            File.WriteAllBytes(fileName, reportContent);
            Console.WriteLine($"Report downloaded to: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading report: {ex.Message}");
        }
    }

    static void CreateDirectoriesFromUrl(string url)
    {
        try
        {
            // Remover caracteres indesejados da URL, como barras no final
            url = url.TrimEnd('\\', '/');

            // Dividir a URL em partes usando barras como delimitadores
            string[] directories = url.Split('\\', '/');

            // Construir e criar os diretórios
            string currentPath = directories[0] + Path.DirectorySeparatorChar;
            foreach (string directory in directories.Skip(1).Take(directories.Length-1))
            {
                currentPath = Path.Combine(currentPath, directory);
                Directory.CreateDirectory(currentPath);
            }

            Console.WriteLine($"Directories created successfully at: {url}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directories: {ex.Message}");
        }
    }


}
