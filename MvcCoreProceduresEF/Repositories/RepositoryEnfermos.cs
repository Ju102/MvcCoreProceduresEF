using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System;
using System.Data;
using System.Data.Common;

namespace MvcCoreProceduresEF.Repositories
{

    #region STORED_PROCEDURES
    //    create procedure SP_ALL_ENFERMOS
    //as
    //	select* from ENFERMO
    //go

    //create procedure SP_FIND_ENFERMO
    //(@inscripcion NVARCHAR(50))
    //as
    //	select* from ENFERMO
    //        where INSCRIPCION = @inscripcion
    //go

    //create procedure SP_DELETE_ENFERMO
    //(@inscripcion NVARCHAR(50))
    //as
    //	delete from ENFERMO
    //        where INSCRIPCION = @inscripcion
    //go
    #endregion

    public class RepositoryEnfermos
    {
        private EnfermosContext context;

        public RepositoryEnfermos(EnfermosContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            // Necesitamos un command, vamos a utilizar un string para todo.
            // El command, en su creación, necesita de una cadena de conexión (objeto).
            // El objeto Connection nos lo ofrece EF. Las conexiones se hacen a partir del context.
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;

                // Abrimos la conexión a través del command
                await com.Connection.OpenAsync();

                // Ejecutamos el reader
                DbDataReader reader = await com.ExecuteReaderAsync();

                // Se debe mapear los datos manualmente
                List<Enfermo> enfermos = new List<Enfermo>();
                while (await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo()
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["APELLIDO"].ToString(),
                        Fecha_Nacimiento = DateTime.Parse(reader["FECHA_NAC"].ToString()),
                        Genero = reader["S"].ToString(),
                        SeguridadSocial = reader["NSS"].ToString()
                    };
                    enfermos.Add(enfermo);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }
    }
}
