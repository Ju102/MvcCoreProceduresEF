using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;

#region STORED_VIEWS_PROCEDURES
//create view V_EMPLEADOS_DEPARTAMENTO
//as
//	select CAST(isnull(ROW_NUMBER() over (order by EMP.APELLIDO),0) as int) as ID, EMP.APELLIDO, EMP.OFICIO, EMP.SALARIO, DEPT.DNOMBRE as DEPARTAMENTO, DEPT.LOC as LOCALIDAD
//		from EMP inner join DEPT on EMP.DEPT_NO = DEPT.DEPT_NO
//go

//create view V_TRABAJADORES
//as
//	select EMP_NO as ID_TRABAJADOR, APELLIDO, OFICIO, SALARIO from EMP
//	union
//	select DOCTOR_NO, APELLIDO, ESPECIALIDAD, SALARIO from DOCTOR
//	union
//	select EMPLEADO_NO, APELLIDO, FUNCION, SALARIO from PLANTILLA
//go

//create procedure SP_TRABAJADORES_OFICIO
//(@oficio nvarchar(50), @personas int out, @media int out, @suma int out)
//as
//	-- Consulta
//	select * from V_TRABAJADORES
//		where OFICIO = @oficio

//	-- Busqueda
//	select @personas = count(ID_TRABAJADOR) from V_TRABAJADORES where OFICIO = @oficio
//	select @media = AVG(SALARIO) from V_TRABAJADORES where OFICIO = @oficio
//	select @suma = SUM(SALARIO) from V_TRABAJADORES where OFICIO = @oficio
//go
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEmpleados
    {
        private readonly HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Asynchronously retrieves a list of employee views from the data source.
        /// </summary>
        ///
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="VistaEmpleado"/> objects representing employee views. The list will be empty if no employee views are
        /// found.</returns>
        public async Task<List<VistaEmpleado>> GetVistasEmpleadosAsync()
        {
            var consulta = from datos in this.context.VistasEmpleados select datos;

            return await consulta.ToListAsync();
        }

        // Primero con LINQ
        public async Task<TrabajadoresModel> GetTrabajadoresModelAsync()
        {
            var consulta = from datos in this.context.Trabajadores select datos;

            TrabajadoresModel model = new TrabajadoresModel();

            model.Trabajadores = await consulta.ToListAsync();
            model.Recuento = await consulta.CountAsync();
            model.SumaSalarial = await consulta.SumAsync(t => t.Salario);
            model.MediaSalarial = (int)await consulta.AverageAsync(t => t.Salario);

            return model;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Trabajadores select datos.Oficio).Distinct();

            return await consulta.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a collection of workers and related salary statistics for the specified job title.
        /// </summary>
        /// <remarks>This method executes a stored procedure to obtain both the list of workers and
        /// aggregate salary data for the given job title. The operation is performed asynchronously and is suitable for
        /// use in scalable applications.</remarks>
        /// <param name="oficio">The job title used to filter workers. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TrabajadoresModel with the
        /// list of workers matching the specified job title and associated salary statistics. If no workers are found,
        /// the list will be empty and statistics will reflect zero values.</returns>
        public async Task<TrabajadoresModel> GetTrabajadoresModelByOficioAsync(string oficio)
        {
            // Ya que tenemos Model, vamos a llamarlo con EF
            // La unica diferencia cuando tenemos parametros de salida es indicar la palabra out en la declaracion de las variables
            string sql = "SP_TRABAJADORES_OFICIO @oficio, @personas OUT, @media OUT, @suma OUT";

            SqlParameter paramOficio = new SqlParameter("@oficio", oficio);
            SqlParameter paramPersonas = new SqlParameter("@personas", -1);
            paramPersonas.Direction = ParameterDirection.Output;
            SqlParameter paramMedia = new SqlParameter("@media", -1);
            paramMedia.Direction = ParameterDirection.Output;
            SqlParameter paramSuma = new SqlParameter("@suma", -1);
            paramSuma.Direction = ParameterDirection.Output;

            // Ejecutamos la consulta con el model FromSqlRaw
            var consulta = this.context.Trabajadores.FromSqlRaw(
                sql,
                paramOficio,
                paramPersonas,
                paramMedia,
                paramSuma
            );

            TrabajadoresModel model = new TrabajadoresModel();

            // Hasta que no leemos los datos(reader.Close()) no se devuelven los parámetros de salida.
            model.Trabajadores = await consulta.ToListAsync();

            model.Recuento = int.Parse(paramPersonas.Value.ToString());
            model.MediaSalarial = int.Parse(paramMedia.Value.ToString());
            model.SumaSalarial = int.Parse(paramSuma.Value.ToString());

            return model;
        }
    }
}
