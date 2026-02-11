using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED_PROCEDURES
    //create procedure SP_ALL_ESPECIALIDADES
    //as
    //	select DISTINCT(ESPECIALIDAD) from DOCTOR
    //go

    //create procedure SP_INCREMENTAR_SALARIO_ESPECIALIDAD
    //(@especialidad NVARCHAR(50), @incremento INT)
    //as
    //	update DOCTOR set SALARIO = (SALARIO + @incremento) where ESPECIALIDAD = @especialidad
    //go

    //create procedure SP_ALL_DOCTORES_ESPECIALIDAD
    //(@especialidad NVARCHAR(50))
    //as
    //	select* from DOCTOR where ESPECIALIDAD = @especialidad
    //go
    #endregion

    public class RepositoryDoctores
    {
        private HospitalContext context;

        public RepositoryDoctores(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetEspecialidades()
        {
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ESPECIALIDADES";

                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;

                await com.Connection.OpenAsync();

                List<string> especialidades = new List<string>();

                DbDataReader reader = await com.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string esp = reader["ESPECIALIDAD"].ToString();
                    especialidades.Add(esp);
                }

                await reader.CloseAsync();
                await com.Connection.CloseAsync();

                return especialidades;
            }
        }

        public async Task<List<Doctor>> GetDoctoresByEspecialidadAsync(string especialidad)
        {
            string sql = "SP_ALL_DOCTORES_ESPECIALIDAD @especialidad";

            SqlParameter paramEsp = new SqlParameter("@especialidad", especialidad);

            var consulta = this.context.Doctores.FromSqlRaw(sql, paramEsp);

            List<Doctor> doctores = await consulta.ToListAsync();

            return doctores;
        }

        public async Task UpdateDoctoresByEspecialidad_sp(string especialidad, int incremento)
        {
            string sql = "SP_INCREMENTAR_SALARIO_ESPECIALIDAD @especialidad, @incremento";
            SqlParameter paramEsp = new SqlParameter("@especialidad", especialidad);
            SqlParameter paramInc = new SqlParameter("@incremento", incremento);

            await this.context.Database.ExecuteSqlRawAsync(sql, paramEsp, paramInc);
        }

        public async Task UpdateDoctoresByEspecialidad_ef(string especialidad, int incremento)
        {
            //List<Doctor> doctores = await this
            //    .context.Doctores.Where(doctor => doctor.Especialidad == especialidad)
            //    .ToListAsync();
            var consulta =
                from datos in this.context.Doctores
                where datos.Especialidad == especialidad
                select datos;
            List<Doctor> doctores = await consulta.ToListAsync();

            foreach (Doctor doctor in doctores)
            {
                doctor.Salario += incremento;
            }

            await this.context.SaveChangesAsync();
        }
    }
}
