namespace ClaraMgmt

open System
open System.Drawing
open System.IO
open System.Management.Automation
open System.Security.Cryptography
open System.Threading.Tasks
open QRCoder

open Store
open System.Diagnostics
open System.Collections.Generic
open Newtonsoft.Json

type NameId = {
    Id: string option
    Name: string
    DisplayName: string
}

type CardMetadata = {
    Created: DateTimeOffset
    ContentLastModified: DateTimeOffset
    CommentLastModified: Nullable<DateTimeOffset>
    ReplyLastModified: Nullable<DateTimeOffset>
}

type ClaraMgmtPSCmdlet () =
    inherit PSCmdlet ()

    member x.NewCardId unit: string =
        let guid = Guid.NewGuid ()
        let hash = SHA1.HashData (guid.ToByteArray ())
        ((BitConverter.ToString hash).Replace ("-", "")).[..7].ToLower()

    member x.GetUrl (id: string): string = sprintf "https://project-clara.com/cards/%s" id

    member x.GenerateQRCode (url: string): Bitmap =
        let qrGenerator: QRCodeGenerator = new QRCodeGenerator ()
        let qrCodeData = qrGenerator.CreateQrCode (url, QRCodeGenerator.ECCLevel.M)
        let qrCode = new QRCode (qrCodeData)
        qrCode.GetGraphic (20, Color.Black, Color.White, false)

    member x.GetNameAndDisplayName (nameToken: string): string * string =
        let tokens = nameToken.Split('(', ')')
        if tokens.Length = 1 then ((tokens.[0]).Trim(), (tokens.[0]).Trim())
        else ((tokens.[0]).Trim(), (tokens.[1]).Trim())

    member x.ParseFolderName (folderName: string): NameId =
        let tokens = folderName.Split('-')
        if tokens.Length <> 2 then invalidArg "folderName" "Invalid folder name"
        else
            let id = (tokens.[0]).Trim()
            let (name, displayName) = x.GetNameAndDisplayName ((tokens.[1]).Trim())
            match id with
            | "#" -> { Id = None; Name = name; DisplayName = displayName }
            | _ -> { Id = Some id; Name = name; DisplayName = displayName }

    member x.GetFolderName (id: string) (name: string) (displayName: string): string =
        sprintf "%s - %s (%s)" id name displayName

    member x.ExportSingleCard (outputPath: string) (qrCode: bool) (card: Card) : Unit =
        let dirInfo = Directory.CreateDirectory (Path.Join (x.GetPath outputPath, x.GetFolderName card.Id card.Name card.DisplayName))
        let contentFile = Path.Join (dirInfo.FullName, "content")
        File.WriteAllText (contentFile, card.Content)
        let commentFile = Path.Join (dirInfo.FullName, "comment")
        File.WriteAllText (commentFile, card.Comment)
        let commentFile = Path.Join (dirInfo.FullName, "reply")
        File.WriteAllText (commentFile, card.Reply)
        let metadata = {
            Created = card.Created
            ContentLastModified = card.ContentLastModified
            CommentLastModified = card.CommentLastModified
            ReplyLastModified = card.ReplyLastModified
        }
        let metadataFile = Path.Join (dirInfo.FullName, "metadata.json")
        File.WriteAllText (metadataFile, JsonConvert.SerializeObject(metadata))
        if qrCode then
            let bitmap = x.GenerateQRCode (x.GetUrl card.Id)
            bitmap.Save (Path.Join (dirInfo.FullName, sprintf "%s.bmp" card.Id))
        else ()

    member x.GetPath (path: string): string =
        if Path.IsPathFullyQualified path then path else Path.Join (x.SessionState.Path.CurrentFileSystemLocation.Path, path)

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

    [<Parameter>]
    member val Cards: SwitchParameter = SwitchParameter (false) with get, set

    [<Parameter>]
    member val Logs: SwitchParameter = SwitchParameter (false) with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()

    override x.ProcessRecord () =
        // Delete then recreate
        if x.Cards.IsPresent then
            x.WriteObject "Clearing cards container"
            x.store.clearCardsContainer().Result
        else ()
        if x.Logs.IsPresent then
            x.WriteObject "Clearing logs container"
            x.store.clearCardsContainer().Result
        else ()

[<Cmdlet(VerbsCommon.Clear, "Logs")>]
type ClearLogs () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, Position = 0)>]
    member val CardId: string = null with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.CardId <- x.CardId.ToLower()
        x.WriteObject (sprintf "Clearing logs for %s" x.CardId)

    override x.ProcessRecord () =
        // Delete then recreate
        (x.store.getLogs null x.CardId null).Result
        |> List.map (fun (log: Log) -> x.store.deleteLog(log.Id).Result) |> ignore

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
    member val DisplayName: string = null with get, set
    
    [<Parameter>]
    member val Content: string = null with get, set

    [<Parameter>]
    member val Overwrite: SwitchParameter = new SwitchParameter (false) with get, set
    
    override x.BeginProcessing () =
        base.BeginProcessing ()
        if String.IsNullOrWhiteSpace x.Id then
            x.Id <- x.NewCardId ()
            // We assume that collisions will not be an issue as there are 2^32 possible ids
            // However, we ensure we avoid overwriting cards in the rare case this happens
            if x.Overwrite.IsPresent then
                x.WriteObject "Id not specified. Setting overwrite to false"
                x.Overwrite <- new SwitchParameter (false)
            else ()

        x.Id <- x.Id.ToLower()
        if String.IsNullOrEmpty x.DisplayName then x.DisplayName <- x.Name else ()
        x.WriteObject (sprintf "Creating new card with id %s" x.Id)

    override x.ProcessRecord () =
        match (x.store.newCard x.Id x.Name x.DisplayName x.Content x.Overwrite.IsPresent).Result with
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
    member val NewDisplayName: string = null with get, set
    
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
                            x.Id x.NewName x.NewDisplayName x.NewContent x.NewReply x.ForceReply.IsPresent x.Create.IsPresent).Result
                        | "UsingName" ->
                            let backupId = x.NewCardId()
                            (x.store.editCardByName
                            x.Name backupId x.NewName x.NewDisplayName x.NewContent x.NewReply x.ForceReply.IsPresent x.Create.IsPresent).Result
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
[<OutputType(typeof<Log>)>]
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

    [<Parameter(Mandatory = true, Position = 0)>]
    member val CardId: string = null with get, set

    [<Parameter>]
    member val OutputFile: string = null with get, set

    [<Parameter>]
    member val Show: SwitchParameter = new SwitchParameter (false) with get, set

    [<DefaultValue>]
    val mutable url: string

    override x.BeginProcessing () =
        base.BeginProcessing ()
        if String.IsNullOrEmpty x.OutputFile then
            x.OutputFile <- sprintf "%s.bmp" x.CardId
            else ()
        x.url <- x.GetUrl x.CardId
        x.WriteObject (sprintf "Generating QR Code for %s" x.url)

    override x.ProcessRecord () =
        let bitmap = x.GenerateQRCode x.url
        bitmap.Save (Path.GetFullPath x.OutputFile)
        if x.Show.IsPresent then
            let p = new Process()
            p.StartInfo <- new ProcessStartInfo(x.OutputFile)
            p.StartInfo.UseShellExecute <- true
            p.Start () |> ignore
        else ()

[<Cmdlet(VerbsData.Import, "Cards")>]
type ImportCards () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, ParameterSetName = "SingleCard")>]
    member val Path: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "MultipleCards")>]
    member val RootPath: string = null with get, set

    [<Parameter>]
    member val Overwrite: SwitchParameter = new SwitchParameter (false) with get, set

    [<DefaultValue>]
    val mutable root: string

    [<DefaultValue>]
    val mutable paths: string list

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "SingleCard" ->
            let actualPath = x.GetPath x.Path
            x.root <- Path.GetPathRoot actualPath
            x.paths <- Path.GetDirectoryName actualPath :: []
            x.WriteObject (sprintf "Importing card at %s" x.Path)
        | "MultipleCards" ->
            let actualPath = x.GetPath x.RootPath
            x.root <- actualPath
            x.paths <- Directory.GetDirectories actualPath |> List.ofArray |> List.map Path.GetFileName
            x.WriteObject (sprintf "Importing %d cards at %s" (List.length x.paths) x.RootPath)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

    override x.ProcessRecord () =
        // Get all cards
        List.map (fun (p: string) ->
            let nameId = x.ParseFolderName p
            let id =
                match nameId.Id with
                | None -> x.NewCardId ()
                | Some i -> i

            let name = nameId.Name
            let displayName = nameId.DisplayName

            let folderName = x.GetFolderName id name displayName
            let path = Path.Join (x.root, folderName)

            if nameId.Id = None then Directory.Move (Path.Join (x.root, p), path) else ()

            let content =
                let contentFile = Path.Join (path, "content")
                try File.ReadAllText contentFile with
                | _ -> ""

            let comment =
                let commentFile = Path.Join (path, "comment")
                try File.ReadAllText commentFile with
                | _ -> null

            let reply =
                let replyFile = Path.Join (path, "reply")
                try File.ReadAllText replyFile with
                | _ -> null

            let metadata =
                try
                    let metadataFile = Path.Join (path, "metadata.json")
                    let metadataText = File.ReadAllText metadataFile
                    Some (JsonConvert.DeserializeObject<CardMetadata>(metadataText))
                    with
                | _ -> None

            let (created, contentLastModified, commentLastModified, replyLastModified) =
                match metadata with
                | Some m -> (m.Created, m.ContentLastModified, m.CommentLastModified, m.ReplyLastModified)
                | _ ->
                    let currentTime = DateTimeOffset.UtcNow
                    (currentTime,
                     currentTime,
                     (if comment = null then Nullable () else Nullable currentTime),
                     (if reply = null then Nullable () else Nullable currentTime))

            let card = {
                Id = id
                Name = name
                DisplayName = displayName
                Content = content
                Comment = comment
                Reply = reply
                Created = created
                ContentLastModified = contentLastModified
                CommentLastModified = commentLastModified
                ReplyLastModified = replyLastModified
            }

            // Upsert
            (x.store.addRawCard card x.Overwrite.IsPresent).Result
        ) x.paths |> ignore

[<Cmdlet(VerbsData.Export, "Cards")>]
type ExportCards () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, ParameterSetName = "UsingId")>]
    member val Id: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "UsingName")>]
    member val Name: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "All")>]
    member val All: SwitchParameter = SwitchParameter(false) with get, set

    [<Parameter>]
    member val OutputPath: string = "" with get, set

    [<Parameter>]
    member val QRCode: SwitchParameter = SwitchParameter(false) with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "UsingId" ->
            x.Id <- x.Id.ToLower()
            x.WriteObject (sprintf "Exporting card with id %s" x.Id)
        | "UsingName" ->
            x.WriteObject (sprintf "Exporting card with name %s" x.Name)
        | "All" ->
            if not x.All.IsPresent then
                x.WriteWarning "Invalid parameters. Please specify card id, name or -All"
                x.StopProcessing ()
            else ()
            x.WriteObject "Exporting all cards"
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

        x.OutputPath <- x.GetPath x.OutputPath

    override x.ProcessRecord () =
        match x.ParameterSetName with
        | "UsingId" ->
            (x.store.getCard x.Id).Result
            |> x.ExportSingleCard x.OutputPath x.QRCode.IsPresent
        | "UsingName" ->
            (x.store.getCardByName x.Name).Result
            |> x.ExportSingleCard x.OutputPath x.QRCode.IsPresent
        | "All" ->
            (x.store.listCards ()).Result
            |> List.map (x.ExportSingleCard x.OutputPath x.QRCode.IsPresent)
            |> ignore
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

[<Cmdlet(VerbsData.Sync, "Cards")>]
type SyncCards () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter(Mandatory = true, ParameterSetName = "SingleCard")>]
    member val Path: string = null with get, set

    [<Parameter(Mandatory = true, ParameterSetName = "MultipleCards")>]
    member val RootPath: string = null with get, set

    [<DefaultValue>]
    val mutable root: string

    [<DefaultValue>]
    val mutable paths: string list

    override x.BeginProcessing () =
        base.BeginProcessing ()
        match x.ParameterSetName with
        | "SingleCard" ->
            let actualPath = x.GetPath x.Path
            x.root <- Path.GetPathRoot actualPath
            x.paths <- Path.GetDirectoryName actualPath :: []
            x.WriteObject (sprintf "Syncing card at %s" x.Path)
        | "MultipleCards" ->
            let actualPath = x.GetPath x.RootPath
            x.root <- actualPath
            x.paths <- Directory.GetDirectories actualPath |> List.ofArray |> List.map Path.GetFileName
            x.WriteObject (sprintf "Syncing %d cards at %s" (List.length x.paths) x.RootPath)
        | _ ->
            x.WriteWarning "Invalid parameter set name"
            x.StopProcessing()

    override x.ProcessRecord () =
        // Get all cards
        let cards = x.store.listCards().Result
        let cardsDict = new Dictionary<string, Card> ()
        List.map (fun (c: Card) -> cardsDict.Add (c.Id, c)) cards |> ignore
        List.map (fun (p: string) ->
            let nameId = x.ParseFolderName p
            let id =
                match nameId.Id with
                | None -> x.NewCardId ()
                | Some i -> i

            let oldCard =
                match cardsDict.ContainsKey id with
                | true -> Some cardsDict.[id]
                | false -> None

            let name = nameId.Name
            let displayName = nameId.DisplayName

            let folderName = x.GetFolderName id name displayName
            let path = Path.Join (x.root, folderName)

            if nameId.Id = None then Directory.Move (Path.Join (x.root, p), path) else ()

            let content =
                let contentFile = Path.Join (path, "content")
                try File.ReadAllText contentFile with
                | _ -> ""

            let comment =
                match oldCard with
                | Some c -> c.Comment
                | None -> null

            if not (String.IsNullOrEmpty comment) then
                let commentFile = Path.Join (path, "comment")
                File.WriteAllText (commentFile, comment)

            let reply =
                let replyFile = Path.Join (path, "reply")
                try File.ReadAllText replyFile with
                | _ -> null

            let currentTime = DateTimeOffset.UtcNow

            let created =
                match oldCard with
                | Some c -> c.Created
                | None -> currentTime

            let contentLastModified =
                match oldCard with
                | Some c -> if c.Content <> content then currentTime else c.ContentLastModified
                | None -> currentTime

            let commentLastModified =
                match oldCard with
                | Some c -> c.CommentLastModified
                | None -> Nullable ()

            let replyLastModified =
                match oldCard with
                | Some c -> if c.Content <> content then Nullable currentTime else c.ReplyLastModified
                | None -> if String.IsNullOrEmpty reply then Nullable currentTime else Nullable ()

            let metadata = {
                Created = created
                ContentLastModified = contentLastModified
                CommentLastModified = commentLastModified
                ReplyLastModified = replyLastModified
            }

            let metadataFile = Path.Join (path, "metadata.json")
            File.WriteAllText (metadataFile, JsonConvert.SerializeObject(metadata))

            let card = {
                Id = id
                Name = name
                DisplayName = displayName
                Content = content
                Comment = comment
                Reply = reply
                Created = created
                ContentLastModified = contentLastModified
                CommentLastModified = commentLastModified
                ReplyLastModified = replyLastModified
            }

            // Upsert
            (x.store.addRawCard card true).Result |> ignore
            cardsDict.Remove id
        ) x.paths |> ignore

        cardsDict.Keys
        |> Seq.iter (fun id -> (x.store.deleteCard id).Result |> ignore)

[<Cmdlet(VerbsData.Import, "Logs")>]
type ImportLogs () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter>]
    member val Path: string = "logs.csv" with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject (sprintf "Importing logs from %s" x.Path)

    override x.ProcessRecord () =
        let lines = File.ReadAllLines (x.GetPath x.Path) |> List.ofArray
        let logs =
            lines |> List.map (fun (line: string) ->
                let items = line.Split(',')
                let id = items.[0]
                let operation = items.[1]
                let cardId = items.[2]
                let accessTime = DateTimeOffset.Parse items.[3]
                let region = items.[4]
                { Id = id; Operation = operation; CardId = cardId; AccessTime = accessTime; Region = region; })
        let tasks = logs |> List.map x.store.addRawLog |> List.toArray
        Task.WhenAll tasks |> ignore

[<Cmdlet(VerbsData.Export, "Logs")>]
type ExportLogs () =
    inherit ClaraMgmtPSCmdlet ()

    [<Parameter>]
    member val OutputPath: string = "logs.csv" with get, set

    override x.BeginProcessing () =
        base.BeginProcessing ()
        x.WriteObject (sprintf "Exporting logs to %s" x.OutputPath)

    override x.ProcessRecord () =
        let result = (x.store.getLogs null null null).Result
        let lines = List.map (fun (l: Log) -> sprintf "%s,%s,%s,%s,%s" l.Id l.Operation l.CardId (l.AccessTime.ToString()) l.Region) result
        File.WriteAllLines (x.GetPath x.OutputPath, lines)
