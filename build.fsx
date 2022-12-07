#r "paket:
version 7.0.2-beta8
framework: net6.0
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.5 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "streetname-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let buildSolution = buildSolution assemblyVersionNumber
let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "StreetNameRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSolution "StreetNameRegistry"
)

Target.create "Test_Solution" (fun _ -> test "StreetNameRegistry")

Target.create "Publish_Solution" (fun _ ->
  [
    "StreetNameRegistry.Projector"
    "StreetNameRegistry.Api.BackOffice"
    "StreetNameRegistry.Api.BackOffice.Abstractions"
    "StreetNameRegistry.Api.BackOffice.Handlers.Lambda"
    "StreetNameRegistry.Api.Legacy"
    "StreetNameRegistry.Api.Oslo"
    "StreetNameRegistry.Api.Extract"
    "StreetNameRegistry.Api.CrabImport"
    "StreetNameRegistry.Producer"
    "StreetNameRegistry.Producer.Snapshot.Oslo"
    "StreetNameRegistry.Consumer"
    "StreetNameRegistry.Migrator.StreetName"
    "StreetNameRegistry.Projections.Legacy"
    "StreetNameRegistry.Projections.Extract"
    "StreetNameRegistry.Projections.LastChangedList"
    "StreetNameRegistry.Projections.Wfs"
    "StreetNameRegistry.Projections.Wms"
    "StreetNameRegistry.Projections.Syndication"
  ] |> List.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [
    "StreetNameRegistry.Projector"
    "StreetNameRegistry.Api.BackOffice"
    "StreetNameRegistry.Api.BackOffice.Abstractions"
    "StreetNameRegistry.Api.Legacy"
    "StreetNameRegistry.Api.Oslo"
    "StreetNameRegistry.Api.Extract"
    "StreetNameRegistry.Api.CrabImport"
    "StreetNameRegistry.Producer"
    "StreetNameRegistry.Producer.Snapshot.Oslo"
    "StreetNameRegistry.Consumer"
    "StreetNameRegistry.Migrator.StreetName"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "StreetNameRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiBackOffice" (fun _ -> containerize "StreetNameRegistry.Api.BackOffice" "api-backoffice")
Target.create "PushContainer_ApiBackOffice" (fun _ -> push "api-backoffice")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "StreetNameRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiOslo" (fun _ -> containerize "StreetNameRegistry.Api.Oslo" "api-oslo")
Target.create "PushContainer_ApiOslo" (fun _ -> push "api-oslo")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "StreetNameRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "StreetNameRegistry.Api.CrabImport" "api-crab-import")
Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_Consumer" (fun _ -> containerize "StreetNameRegistry.Consumer" "consumer")
Target.create "PushContainer_Consumer" (fun _ -> push "consumer")

Target.create "Containerize_Producer" (fun _ -> containerize "StreetNameRegistry.Producer" "producer")
Target.create "PushContainer_Producer" (fun _ -> push "producer")

Target.create "Containerize_Producer_Snapshot_Oslo" (fun _ -> containerize "StreetNameRegistry.Producer.Snapshot.Oslo" "producer-snapshot-oslo")
Target.create "PushContainer_Producer_Snapshot_Oslo" (fun _ -> push "producer-snapshot-oslo")

Target.create "Containerize_Migrator_StreetName" (fun _ -> containerize "StreetNameRegistry.Migrator.StreetName" "migrator-streetname")
Target.create "PushContainer_Migrator_StreetName" (fun _ -> push "migrator-streetname")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "StreetNameRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_Projector"
  ==> "Containerize_ApiBackOffice"
  ==> "Containerize_ApiLegacy"
  ==> "Containerize_ApiOslo"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_Consumer"
  ==> "Containerize_Producer"
  ==> "Containerize_Producer_Snapshot_Oslo"
  ==> "Containerize_Migrator_StreetName"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiBackOffice"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiOslo"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_Consumer"
  ==> "PushContainer_Producer"
  ==> "PushContainer_Producer_Snapshot_Oslo"
  ==> "PushContainer_Migrator_StreetName"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
