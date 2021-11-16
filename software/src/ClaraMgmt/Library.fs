namespace ClaraMgmt

open FSharp.Control.Tasks
open System
open System.Management.Automation
open System.Threading.Tasks

open Shared

type ClaraMgmtPSCmdlet () =
    inherit PSCmdlet ()

    // Related to getting Cosmos DB connection string
    [<Parameter>]
    member val KeyVaultUrl: string = Defaults.KeyVaultUrl with get, set

    [<Parameter>]
    member val KeyVaultItemName: string = Defaults.KeyVaultItemName with get, set

    [<Parameter>]
    member val UseEnvironmentVariable: bool = true with get, set

    [<Parameter>]
    member val ConnectionString: string = "" with get, set

    [<Parameter>]
    member val SaveConnectionString: bool = true with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()

        if (String.IsNullOrEmpty x.ConnectionString) then
            x.ConnectionString <- (Secrets.getConnectionString x.UseEnvironmentVariable x.KeyVaultUrl x.KeyVaultItemName).Result
            else ()

        if x.SaveConnectionString then
            Environment.SetEnvironmentVariable ("connectionString", x.ConnectionString)
            else ()

[<Cmdlet(VerbsCommon.Get, "ConnectionString")>]
type GetConnectionString () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        printfn "Connection string: %s" x.ConnectionString

[<Cmdlet(VerbsCommon.Clear, "ConnectionString")>]
type ClearConnectionString () =
    inherit PSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        Environment.SetEnvironmentVariable ("connectionString", "")

[<Cmdlet(VerbsData.Initialize, "ClaraDB")>]
type InitializeClaraDB () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        let store = new Store (x.ConnectionString)
        store.createDB.Result

[<Cmdlet(VerbsCommon.Remove, "ClaraDB")>]
type DeleteClaraDB () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        let store = new Store (x.ConnectionString)
        store.deleteDB.Result
