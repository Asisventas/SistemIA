using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SistemIA.Models;

#nullable disable

namespace SistemIA.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250829173731_Drop_Legacy_Ciudades_Distritos")]
    partial class Drop_Legacy_Ciudades_Distritos
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            // Intencionalmente vacío: esta migración se usa sólo como no-op marcador.
        }
    }
}
