using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class DoctoresController : Controller
    {
        private RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["especialidades"] = await this.repo.GetEspecialidades();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string especialidad, int incremento, string action)
        {
            ViewData["especialidades"] = await this.repo.GetEspecialidades();

            if (action == "SP")
            {
                await this.repo.UpdateDoctoresByEspecialidad_sp(especialidad, incremento);
            }
            else
            {
                await this.repo.UpdateDoctoresByEspecialidad_ef(especialidad, incremento);
            }
            List<Doctor> doctores = await this.repo.GetDoctoresByEspecialidadAsync(especialidad);

            return View(doctores);
        }
    }
}
