﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreetNameRegistry.Projections.Extract;

namespace StreetNameRegistry.Projections.Extract.Migrations
{
    [DbContext(typeof(ExtractContext))]
    [Migration("20190417131116_AddLanguageNames")]
    partial class AddLanguageNames
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Position");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","StreetNameRegistryExtract");
                });

            modelBuilder.Entity("StreetNameRegistry.Projections.Extract.StreetNameExtract.StreetNameExtractItem", b =>
                {
                    b.Property<Guid?>("StreetNameId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Complete");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<string>("NameDutch");

                    b.Property<string>("NameEnglish");

                    b.Property<string>("NameFrench");

                    b.Property<string>("NameGerman");

                    b.Property<string>("NameUnknown");

                    b.Property<int>("StreetNameOsloId");

                    b.HasKey("StreetNameId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("StreetNameOsloId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("StreetName","StreetNameRegistryExtract");
                });
#pragma warning restore 612, 618
        }
    }
}