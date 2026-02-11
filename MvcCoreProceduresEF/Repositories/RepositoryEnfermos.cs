using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED_PROCEDURES
    //create procedure sp_all_enfermos
    //as
    //	select* from enfermo
    //go

    //create procedure sp_find_enfermo
    //(@inscripcion nvarchar(50))
    //as
    //	select* from enfermo
    //        where inscripcion = @inscripcion
    //go

    //create procedure sp_delete_enfermo
    //(@inscripcion nvarchar(50))
    //as
    //	delete from enfermo
    //        where inscripcion = @inscripcion
    //go

    //create procedure SP_INSERT_ENFERMO
    //(@apellido NVARCHAR(50), @direccion NVARCHAR(50), @fecha DATETIME, @genero NVARCHAR(50), @nss NVARCHAR(50))
    //as
    //	declare @maxins INT
    //  select @maxins = (CAST(MAX(INSCRIPCION) as INT) + 1) from ENFERMO
    //  insert into ENFERMO values(CAST(@maxins as NVARCHAR(50)), @apellido, @direccion, @fecha, @genero, @nss)
    //go

    #endregion

    public class RepositoryEnfermos
    {
        private HospitalContext context;

        public RepositoryEnfermos(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            // Necesitamos un command, vamos a utilizar un string para todo.
            // El command, en su creación, necesita de una cadena de conexión (objeto).
            // El objeto Connection nos lo ofrece EF. Las conexiones se hacen a partir del context.
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
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
                        SeguridadSocial = reader["NSS"].ToString(),
                    };
                    enfermos.Add(enfermo);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }

        public async Task<Enfermo> FindEnfermoAsync(string inscripcion)
        {
            // Para llamar a un procedimiento que contiene parámetros. La llamada se realiza mediante el nombre del procedure,
            // y cada parámetro a continuación en la declaración del SQL.
            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter paramIns = new SqlParameter("@inscripcion", inscripcion);

            // Si los datos que devuelve el procedure están mapeados con un Model, podemos utilizar el método FromSqlRaw
            // para recuperar directamente el Model.
            var consulta = this.context.Enfermos.FromSqlRaw(sql, paramIns);
            // var consulta = this.context.Enfermos.FromSqlRaw(sql, paramIns).ToListAsync();

            // Debemos usar AsEnumerable()
            Enfermo enfermo = await consulta.AsAsyncEnumerable().FirstOrDefaultAsync();
            // Enfermo enfermo = consulta.FirstOrDefault();

            return enfermo;
        }

        public async Task CreateEnfermoAsync(
            string apellido,
            string direccion,
            DateTime fecha,
            string genero,
            string nss
        )
        {
            string sql = "SP_INSERT_ENFERMO @apellido, @direccion, @fecha, @genero, @nss";

            SqlParameter paramApellido = new SqlParameter("@apellido", apellido);
            SqlParameter paramDireccion = new SqlParameter("@direccion", direccion);
            SqlParameter paramFecha = new SqlParameter("@fecha", fecha);
            SqlParameter paramGenero = new SqlParameter("@genero", genero);
            SqlParameter paramNss = new SqlParameter("@nss", nss);

            await this.context.Database.ExecuteSqlRawAsync(
                sql,
                paramApellido,
                paramDireccion,
                paramFecha,
                paramGenero,
                paramNss
            );
        }

        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO";

            SqlParameter paramIns = new SqlParameter("@inscripcion", inscripcion);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIns);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }

        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO @inscripcion";

            SqlParameter paramIns = new SqlParameter("@inscripcion", inscripcion);
            await this.context.Database.ExecuteSqlRawAsync(sql, paramIns);
        }
    }
}
