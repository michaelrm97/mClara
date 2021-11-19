namespace ClaraMgmt

open System
open System.Management.Automation
open System.Security.Cryptography

open Shared
open Store

module Helpers =
    let NewCardId unit: string =
        let guid = Guid.NewGuid ()
        let hash = SHA1.HashData (guid.ToByteArray ())
        ((BitConverter.ToString hash).Replace ("-", "")).[..7]

type ClaraMgmtPSCmdlet () =
    inherit PSCmdlet ()

    // Related to getting Cosmos DB connection string
    [<Parameter>]
    member val KeyVaultUrl: string = Defaults.KeyVaultUrl with get, set

    [<Parameter>]
    member val KeyVaultItemName: string = Defaults.KeyVaultItemName with get, set

    [<Parameter>]
    member val DontUseEnvironmentVariable: SwitchParameter = new SwitchParameter(false) with get, set

    [<Parameter>]
    member val ConnectionString: string = "" with get, set

    [<Parameter>]
    member val DontSaveConnectionString: SwitchParameter = new SwitchParameter(false) with get, set

    [<DefaultValue>]
    val mutable store: Store

    override x.BeginProcessing () =
        base.BeginProcessing ()

        if (String.IsNullOrWhiteSpace x.ConnectionString) then
            x.ConnectionString <-
                (Secrets.getConnectionString
                    (not x.DontUseEnvironmentVariable.IsPresent)
                    x.KeyVaultUrl
                    x.KeyVaultItemName
                    (not x.DontSaveConnectionString.IsPresent)).Result
            else ()

        x.store <- new Store (x.ConnectionString) 

[<Cmdlet(VerbsCommon.Get, "ConnectionString")>]
type GetConnectionString () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject (sprintf "Connection string: %s" x.ConnectionString)

[<Cmdlet(VerbsCommon.Clear, "ConnectionString")>]
type ClearConnectionString () =
    inherit PSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Clearing connection string"
        Environment.SetEnvironmentVariable ("connectionString", "")

[<Cmdlet(VerbsData.Initialize, "ClaraDB")>]
type InitializeClaraDB () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Creating database and containers"

    override x.ProcessRecord () =
        x.store.createDB().Result

[<Cmdlet(VerbsCommon.Remove, "ClaraDB")>]
type RemoveClaraDB () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Removing database and containers"

    override x.ProcessRecord () =
        x.store.deleteDB().Result

[<Cmdlet(VerbsCommon.Clear, "ClaraDB")>]
type ClearClaraDB () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Clearing database and containers"

    override x.ProcessRecord () =
        // Delete then recreate
        x.store.deleteDB().Result
        x.store.createDB().Result

[<Cmdlet(VerbsCommon.Get, "Cards")>]
[<OutputType(typeof<Card>)>]
type GetCards () =
    inherit ClaraMgmtPSCmdlet ()

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Getting all cards"

    override x.ProcessRecord () =
        x.WriteObject (x.store.listCards().Result, true)

[<Cmdlet(VerbsCommon.Get, "Card", DefaultParameterSetName="UsingId")>]
[<OutputType(typeof<Card>)>]
type GetCard () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Position = 0, Mandatory = true, ParameterSetName = "UsingId")>]
    member val Id: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "UsingName")>]
    member val Name: string = null with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "UsingId" ->
            x.Id <- x.Id.ToLower()
            x.WriteObject (sprintf "Getting card with id %s" x.Id)
        | "UsingName" -> x.WriteObject (sprintf "Getting card with name %s" x.Name)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

    override x.ProcessRecord () =
        match x.ParameterSetName with
        | "UsingId" -> x.WriteObject (x.store.getCard(x.Id).Result)
        | "UsingName" -> x.WriteObject (x.store.getCardByName(x.Name).Result)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

[<Cmdlet(VerbsCommon.New, "Card")>]
type NewCard () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter>]
    member val Id: string = null with get, set
    
    [<Parameter(Mandatory = true)>]
    member val Name: string = null with get, set
    
    [<Parameter>]
    member val Content: string = null with get, set

    [<Parameter>]
    member val Overwrite: SwitchParameter = new SwitchParameter (false) with get, set
    
    override x.BeginProcessing () =
        base.BeginProcessing ()
        if String.IsNullOrWhiteSpace x.Id then
            x.Id <- Helpers.NewCardId ()
            // We assume that collisions will not be an issue as there are 2^32 possible ids
            // However, we ensure we avoid overwriting cards in the rare case this happens
            x.Overwrite <- new SwitchParameter (false)
            else ()

        x.Id <- x.Id.ToLower()
        x.WriteObject (sprintf "Creating new card with id %s" x.Id)

    override x.ProcessRecord () =
        match (x.store.newCard x.Id x.Name x.Content x.Overwrite.IsPresent).Result with
        | true -> x.WriteObject "Successfully created card"
        | false -> x.WriteObject "Failed to create card due to conflicting id"

[<Cmdlet(VerbsData.Edit, "Card")>]
type EditCard () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, ParameterSetName = "UsingId")>]
    member val Id: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "UsingName")>]
    member val Name: string = null with get, set
    
    [<Parameter>]
    member val NewName: string = null with get, set
    
    [<Parameter>]
    member val NewContent: string = null with get, set

    [<Parameter>]
    member val NewReply: string = null with get, set

    [<Parameter>]
    member val ForceReply: SwitchParameter = new SwitchParameter(false) with get, set

    [<Parameter>]
    member val Create: SwitchParameter = new SwitchParameter (false) with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "UsingId" ->
            x.Id <- x.Id.ToLower()
            x.WriteObject (sprintf "Editing card with id %s" x.Id)
        | "UsingName" -> x.WriteObject (sprintf "Editing card with name %s" x.Name)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

    override x.ProcessRecord () =
        let result = match x.ParameterSetName with
                        | "UsingId" ->
                            (x.store.editCard
                            x.Id x.NewName x.NewContent x.NewReply x.ForceReply.IsPresent x.Create.IsPresent).Result
                        | "UsingName" ->
                            let backupId = (Helpers.NewCardId()).ToLower()
                            (x.store.editCardByName
                            x.Name backupId x.NewName x.NewContent x.NewReply x.ForceReply.IsPresent x.Create.IsPresent).Result
                        | _ ->
                            x.WriteWarning "Invalid parameter set name"
                            false

        match result with
        | true -> x.WriteObject "Successfully edited card"
        | false -> x.WriteObject "Failed to edit card"

[<Cmdlet(VerbsCommon.Remove, "Card")>]
type RemoveCard () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, ParameterSetName = "UsingId")>]
    member val Id: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "UsingName")>]
    member val Name: string = null with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "UsingId" ->
            x.Id <- x.Id.ToLower()
            x.WriteObject (sprintf "Removing card with id %s" x.Id)
        | "UsingName" -> x.WriteObject (sprintf "Removing card with name %s" x.Name)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

    override x.ProcessRecord () =
        let result = match x.ParameterSetName with
                        | "UsingId" -> x.store.deleteCard(x.Id).Result
                        | "UsingName" -> x.store.deleteCardByName(x.Name).Result
                        | _ ->
                            x.WriteWarning "Invalid parameter set name"
                            false

        match result with
        | true -> x.WriteObject "Successfully removed card"
        | false -> x.WriteObject "Failed to remove card"

[<Cmdlet(VerbsCommon.Get, "Logs")>]
type GetLogs () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter>]
    member val Operation: string = null with get, set

    [<Parameter>]
    member val CardId: string = null with get, set

    [<Parameter>]
    member val Region: string = null with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject "Getting logs"

    override x.ProcessRecord () =
        let result = (x.store.getLogs x.Operation x.CardId x.Region).Result
        x.WriteObject (result, true)

[<Cmdlet(VerbsCommon.Get, "QRCode")>]
type GetQRCode () =
    inherit ClaraMgmtPSCmdlet ()
