namespace MvcCoreProceduresEF.Models
{
    public class TrabajadoresModel
    {
        public int Recuento { get; set; }
        public int MediaSalarial { get; set; }
        public int SumaSalarial { get; set; }

        public List<Trabajador> Trabajadores { get; set; }

        public TrabajadoresModel()
        {
            this.Trabajadores = new List<Trabajador>();
        }
    }
}
