using System;
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

        // Define o caminho da pasta onde os DataSources estão localizados
        string folderPath = "/DataSources";

        // Exemplo: Obter conteúdo de um DataSource
        string dataSourcePath = $"{folderPath}/FonteTeste";
        await GetAndPrintDataSourceContent(dataSourcePath);

        // Testar conexão do DataSource
        bool testResult = await TestDataSourceConnection(dataSourcePath, "admin", "senha@123");
        Console.WriteLine($"Resultado do teste de conexão para {dataSourcePath}: {testResult}");

        // Atualizar senha do DataSource
        await UpdateDataSourcePasswordAsync(dataSourcePath, "senha@123");

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

        // Definir o número máximo de propriedades por linha
        const int propertiesPerRow = 13;

        // Dividir as propriedades em grupos
        var propertyGroups = properties.Select((p, index) => new { Index = index, Property = p })
                                       .GroupBy(x => x.Index / propertiesPerRow, x => x.Property)
                                       .ToList();

        // Criar a tabela usando o ConsoleTables
        var table = new ConsoleTable(properties.Select(p => p.Name).ToArray());

        // Adicionar linhas à tabela com os valores das propriedades
        foreach (var propertyGroup in propertyGroups)
        {
            var rowData = new object[propertiesPerRow];

            for (int i = 0; i < propertiesPerRow; i++)
            {
                var propertyIndex = propertyGroup.ElementAtOrDefault(i);

                if (propertyIndex != null)
                {
                    rowData[i] = propertyIndex.GetValue(dataObject);
                }
            }
            table.AddRow(rowData);
        }

        // Imprimir a tabela
        Console.WriteLine(table.ToString());
    }




}
