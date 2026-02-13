using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly RepositoryEmpleados repo;

        public EmpleadosController(RepositoryEmpleados repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<VistaEmpleado> vistasEmpleados = await this.repo.GetVistasEmpleadosAsync();
            return View(vistasEmpleados);
        }
    }
}
