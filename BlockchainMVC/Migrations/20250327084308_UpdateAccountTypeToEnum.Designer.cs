﻿// <auto-generated />
using System;
using BlockchainMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BlockchainMVC.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250327084308_UpdateAccountTypeToEnum")]
    partial class UpdateAccountTypeToEnum
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BlockchainMVC.Models.Crop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateHarvested")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DatePlanted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("sensorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("sensorId");

                    b.ToTable("Crops");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DatePlanted = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Name = "Lúa 1",
                            sensorId = 1
                        },
                        new
                        {
                            Id = 2,
                            DatePlanted = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Name = "Ngô 1",
                            sensorId = 2
                        },
                        new
                        {
                            Id = 3,
                            DatePlanted = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Name = "Khoai tây 1",
                            sensorId = 3
                        });
                });

            modelBuilder.Entity("BlockchainMVC.Models.Sensor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sensorCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("sensorCode")
                        .IsUnique();

                    b.ToTable("Sensors");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Cảm biến nhiệt độ 1",
                            sensorCode = "TEMP001"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Cảm biến độ ẩm 1",
                            sensorCode = "HUM001"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Cảm biến ánh sáng 1",
                            sensorCode = "LIGHT001"
                        });
                });

            modelBuilder.Entity("BlockchainMVC.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountType")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BlockchainMVC.Models.Crop", b =>
                {
                    b.HasOne("BlockchainMVC.Models.Sensor", "Sensor")
                        .WithMany("Crops")
                        .HasForeignKey("sensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");
                });

            modelBuilder.Entity("BlockchainMVC.Models.Sensor", b =>
                {
                    b.Navigation("Crops");
                });
#pragma warning restore 612, 618
        }
    }
}
