open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers
open Store

initializeContext()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
    run dotnet "fable clean --yes" clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable -o output -s --run webpack -p" clientPath ]
    |> runParallel
)

type Region = {
    Name: string
    RegionName: string
    Location: Location
}

Target.create "Azure" (fun _ ->
    let connectionString = (Secrets.getConnectionString false "https://clara-keys.vault.azure.net/" "ConnectionString" true).Result

    let regions: Region list = [
        {
            Name = "clara-wus2"
            RegionName = "West US 2"
            Location = Location.WestUS2
        }
        {
            Name = "clara-aue"
            RegionName = "Australia East"
            Location = Location.AustraliaEast
        }
    ]

    let deployments = regions |> List.map (fun (region: Region) ->
        arm {
            location region.Location
            add_resource (webApp {
                name region.Name
                zip_deploy "deploy"
                sku (WebApp.Basic "B1")
                operating_system OS.Linux
                runtime_stack Runtime.DotNet50
                automatic_logging_extension false
                app_insights_off
                settings [
                    "Region", region.RegionName
                    "ConnectionString", connectionString
                    "Port", "8080"
                ]
                system_identity
                https_only
            })
        })

    deployments
    |> List.iter (fun deployment ->
        deployment
        |> Deploy.execute "project-clara" Deploy.NoParameters
        |> ignore)
)

Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run webpack-dev-server" clientPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"

    "Clean"
        ==> "InstallClient"
        ==> "Run"
]

[<EntryPoint>]
let main args = runOrDefault args