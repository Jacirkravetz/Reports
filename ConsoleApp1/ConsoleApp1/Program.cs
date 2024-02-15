//using System;
//using System.Collections.Generic;
//using System.ServiceModel;
//using System.Threading.Tasks;
//using ServiceReference1;

//namespace RSWcf
//{
//    class Program
//    {
//        static ReportingService2010SoapClient rsclient = null;

//        public static void Main(string[] args)
//        {
//            BasicHttpBinding rsBinding = new BasicHttpBinding();
//            rsBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
//            rsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

//            EndpointAddress rsEndpointAddress = new EndpointAddress("http://desktop-hu97nsf/reportserver/ReportService2010.asmx");

//            rsclient = new ReportingService2010SoapClient(rsBinding, rsEndpointAddress);

//            var output = rsListChildren("/");
//            output.Wait();

//            if (output.Status == TaskStatus.RanToCompletion && output.Result.Length > 0)
//            {
//                foreach (CatalogItem item in output.Result)
//                {
//                    Console.WriteLine($"Item Path: {item.Path}");
//                }
//            }

//            // var testConnectOutput = await TestDataSourceConnection("/DataSources/FonteTeste ", "admin", "senha@UBS");

//            // await UpdateDataSourcePasswordAsync("/DataSources/FonteTeste ", "senha@UBS");


//            //var dataSourceOutput = ListDataSources("/");
//            //dataSourceOutput.Wait();

//            //if (dataSourceOutput.Status == TaskStatus.RanToCompletion && dataSourceOutput.Result.Length > 0)
//            //{
//            //    foreach (DataSourcePrompt dataSource in dataSourceOutput.Result)
//            //    {
//            //        Console.WriteLine($"Data Source Name: {dataSource.Name}");

//            //        //// Testing DataSource connection before updating password
//            //        //var testConnectOutput = TestDataSourceConnection(dataSource.Path, "TestUserName", "TestPassword");
//            //        //testConnectOutput.Wait();

//            //        //if (testConnectOutput.Status == TaskStatus.RanToCompletion && testConnectOutput.Result)
//            //        //{
//            //        //    Console.WriteLine("DataSource connection test passed. Proceeding with password update.");

//            //        //    // Update DataSource password
//            //        //    var updateDataSourceOutput = UpdateDataSourcePasswordAsync(dataSource.Path, "NewPassword");
//            //        //    updateDataSourceOutput.Wait();
//            //        //}
//            //        //else
//            //        //{
//            //        //    Console.WriteLine("DataSource connection test failed. Skipping password update.");
//            //        //}
//            //    }
//            //}

//            //TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
//            Console.WriteLine("Completed!");
//            Console.ReadLine();
//        }

//        private static async Task<CatalogItem[]> rsListChildren(string itemPath)
//        {
//            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
//            ListChildrenResponse listChildrenResponse = null;

//            try
//            {
//                listChildrenResponse = await rsclient.ListChildrenAsync(trustedUserHeader, itemPath, false);
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine($"Error listing children: {exception.Message}");
//                return new CatalogItem[0];
//            }

//            return listChildrenResponse.CatalogItems;
//        }

//        private static async Task<DataSourcePrompt[]> ListDataSources(string itemPath)
//        {
//            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
//            // ListDataSourcesResponse listDataSourcesResponse = null;
//            GetItemDataSourcePromptsResponse listDataSourcesResponse;
//            try
//            {
//                listDataSourcesResponse = await rsclient.GetItemDataSourcePromptsAsync(trustedUserHeader, itemPath);
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine($"Error listing data sources: {exception.Message}");
//                return new DataSourcePrompt[0];
//            }

//            return listDataSourcesResponse.DataSourcePrompts;
//        }

//        private static async Task<bool> TestDataSourceConnection(string dataSourcePath, string userName, string password)
//        {
//            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();

//            // Get DataSource contents
//            var getDataSourceResponse = await rsclient.GetDataSourceContentsAsync(trustedUserHeader, dataSourcePath);
//            DataSourceDefinition dataSourceDefinition = getDataSourceResponse.Definition;



//            TestConnectForDataSourceDefinitionRequest connectForDataSourceDefinitionRequest = new TestConnectForDataSourceDefinitionRequest();
//            connectForDataSourceDefinitionRequest.TrustedUserHeader = trustedUserHeader;
//            connectForDataSourceDefinitionRequest.UserName = userName;
//            connectForDataSourceDefinitionRequest.Password = password;
//            connectForDataSourceDefinitionRequest.DataSourceDefinition = dataSourceDefinition;

//            try
//            {
//                // Test the connection for the given DataSource
//                var testConnectResponse = await rsclient.TestConnectForDataSourceDefinitionAsync(connectForDataSourceDefinitionRequest);


//                return testConnectResponse.TestConnectForDataSourceDefinitionResult;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error testing DataSource connection: {ex.Message}");
//                return false;
//            }
//        }

//        private static async System.Threading.Tasks.Task UpdateDataSourcePasswordAsync(string dataSourcePath, string newPassword)
//        {
//            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
//            try
//            {
//                // Get DataSource contents
//                var getDataSourceResponse = await rsclient.GetDataSourceContentsAsync(trustedUserHeader, dataSourcePath);
//                DataSourceDefinition dataSourceDefinition = getDataSourceResponse.Definition;

//                // Update the password in the DataSourceDefinition
//                dataSourceDefinition.Password = newPassword;

//                // Set updated DataSource contents
//                var setDataSourceResponse = await rsclient.SetDataSourceContentsAsync(trustedUserHeader, dataSourcePath, dataSourceDefinition);

//                Console.WriteLine($"DataSource password updated successfully for {dataSourcePath}.");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error updating DataSource password: {ex.Message}");
//            }
//        }
//    }
//}
