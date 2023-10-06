#r "paket:
version 7.0.2-beta8
framework: net6.0
source https://api.nuget.org/v3/index.json

nuget Microsoft.Build 17.3.2
nuget Microsoft.Build.Framework 17.3.2
nuget Microsoft.Build.Tasks.Core 17.3.2
nuget Microsoft.Build.Utilities.Core 17.3.2
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.6 //"

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

Target.create "Test_Solution" (fun _ ->
    [
        "test" @@ "StreetNameRegistry.Tests"
    ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [|"StreetNameRegistry.Projector"
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
    "StreetNameRegistry.Projections.BackOffice"
    "StreetNameRegistry.Projections.Syndication"
    "StreetNameRegistry.Snapshot.Verifier"
    "StreetNameRegistry.Consumer.Read.Postal"
  |] |> Array.Parallel.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [|"StreetNameRegistry.Api.BackOffice"
    "StreetNameRegistry.Api.BackOffice.Abstractions"
    "StreetNameRegistry.Api.Legacy"
    "StreetNameRegistry.Api.Oslo"
    "StreetNameRegistry.Api.Extract"
    "StreetNameRegistry.Api.CrabImport"
  |] |> Array.Parallel.iter pack)

type ContainerObject = { Project: string; Container: string }

Target.create "Containerize" (fun _ ->
  [|{ Project = "StreetNameRegistry.Projector"; Container = "projector" }
    { Project = "StreetNameRegistry.Api.BackOffice"; Container = "api-backoffice" }
    { Project = "StreetNameRegistry.Api.Legacy"; Container = "api-legacy" }
    { Project = "StreetNameRegistry.Api.Oslo"; Container = "api-oslo" }
    { Project = "StreetNameRegistry.Api.Extract"; Container = "api-extract" }
    { Project = "StreetNameRegistry.Api.CrabImport"; Container = "api-crab-import" }
    { Project = "StreetNameRegistry.Consumer"; Container = "consumer" }
    { Project = "StreetNameRegistry.Producer"; Container = "producer" }
    { Project = "StreetNameRegistry.Producer.Snapshot.Oslo"; Container = "producer-snapshot-oslo" }
    { Project = "StreetNameRegistry.Migrator.StreetName"; Container = "migrator-streetname" }
    { Project = "StreetNameRegistry.Projections.Syndication"; Container = "projections-syndication" }
    { Project = "StreetNameRegistry.Projections.BackOffice"; Container = "projections-backoffice" }
    { Project = "StreetNameRegistry.Snapshot.Verifier"; Container = "snapshot-verifier" }
    { Project = "StreetNameRegistry.Consumer.Read.Postal"; Container = "consumer-read-postal" }
  |] |> Array.Parallel.iter (fun o -> containerize o.Project o.Container))

Target.create "SetAssemblyVersions" (fun _ -> setVersions "SolutionInfo.cs")

Target.create "Containerize_Projector" (fun _ -> containerize "StreetNameRegistry.Projector" "projector")
Target.create "Containerize_ApiBackOffice" (fun _ -> containerize "StreetNameRegistry.Api.BackOffice" "api-backoffice")
Target.create "Containerize_ApiLegacy" (fun _ -> containerize "StreetNameRegistry.Api.Legacy" "api-legacy")
Target.create "Containerize_ApiOslo" (fun _ -> containerize "StreetNameRegistry.Api.Oslo" "api-oslo")
Target.create "Containerize_ApiExtract" (fun _ -> containerize "StreetNameRegistry.Api.Extract" "api-extract")
Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "StreetNameRegistry.Api.CrabImport" "api-crab-import")
Target.create "Containerize_Consumer" (fun _ -> containerize "StreetNameRegistry.Consumer" "consumer")
Target.create "Containerize_Producer" (fun _ -> containerize "StreetNameRegistry.Producer" "producer")
Target.create "Containerize_ProducerSnapshotOslo" (fun _ -> containerize "StreetNameRegistry.Producer.Snapshot.Oslo" "producer-snapshot-oslo")
Target.create "Containerize_MigratorStreetName" (fun _ -> containerize "StreetNameRegistry.Migrator.StreetName" "migrator-streetname")
Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "StreetNameRegistry.Projections.Syndication" "projections-syndication")
Target.create "Containerize_ProjectionsBackOffice" (fun _ -> containerize "StreetNameRegistry.Projections.BackOffice" "projections-backoffice")
Target.create "Containerize_SnapshotVerifier" (fun _ -> containerize "StreetNameRegistry.Snapshot.Verifier" "snapshot-verifier")
Target.create "Containerize_ConsumerReadPostal" (fun _ -> containerize "StreetNameRegistry.Consumer.Read.Postal" "consumer-read-postal")
// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
//Target.create "Containerize" ignore

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
  // ==> "Containerize_Projector"
  // ==> "Containerize_ApiBackOffice"
  // ==> "Containerize_ApiLegacy"
  // ==> "Containerize_ApiOslo"
  // ==> "Containerize_ApiExtract"
  // ==> "Containerize_ApiCrabImport"
  // ==> "Containerize_Consumer"
  // ==> "Containerize_Producer"
  // ==> "Containerize_Producer_Snapshot_Oslo"
  // ==> "Containerize_Migrator_StreetName"
  // ==> "Containerize_ProjectionsSyndication"
  // ==> "Containerize_ProjectionsBackOffice"
  ==> "Containerize"
// Possibly add more projects to containerize here

// By default we build & test
Target.runOrDefault "Test"
