namespace Store

open Azure.Identity
open Azure.Security.KeyVault.Secrets
open FSharp.Control.Tasks
open System
open System.Threading.Tasks

module Secrets =
    let connectionStringEnvName = "ConnectionString"

    let getConnectionStringFromKeyVault (keyVaultUrl: string) (keyVaultItemName: string): Task<string> =
        task {
            let secretClient = new SecretClient (new Uri (keyVaultUrl), new DefaultAzureCredential ())
            let! secret = secretClient.GetSecretAsync (keyVaultItemName)
            return secret.Value.Value
        }

    let getConnectionString (useEnvironmentVariable: bool) (keyVaultUrl: string) (keyVaultItemName: string) (save: bool): Task<string> =
        task {
            let envConnectionString: string =
                if useEnvironmentVariable then
                    Environment.GetEnvironmentVariable (connectionStringEnvName)
                    else null

            let! connectionString =
                (if String.IsNullOrWhiteSpace envConnectionString then
                    (getConnectionStringFromKeyVault keyVaultUrl keyVaultItemName)
                    else Task.FromResult envConnectionString)

            if save then
                Environment.SetEnvironmentVariable (connectionStringEnvName, connectionString)
            else ()

            return connectionString
        }
