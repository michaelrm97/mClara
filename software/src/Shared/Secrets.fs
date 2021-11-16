namespace Shared

open Azure.Identity
open Azure.Security.KeyVault.Secrets
open FSharp.Control.Tasks
open System
open System.Threading.Tasks

module Secrets =
    let _getConnectionStringFromKeyVault (keyVaultUrl: string) (keyVaultItemName: string): Task<string> =
        task {
            let secretClient = new SecretClient (new Uri (keyVaultUrl), new DefaultAzureCredential ())
            let! secret = secretClient.GetSecretAsync (keyVaultItemName)
            return secret.Value.Value
        }

    let getConnectionString (useEnvironmentVariable: bool) (keyVaultUrl: string) (keyVaultItemName: string): Task<string> =
        task {
            let envConnectionString: string =
                if useEnvironmentVariable then
                    Environment.GetEnvironmentVariable("connectionString")
                    else null

            return!
                (if String.IsNullOrWhiteSpace envConnectionString then
                    (_getConnectionStringFromKeyVault keyVaultUrl keyVaultItemName)
                    else Task.FromResult envConnectionString)
        }
