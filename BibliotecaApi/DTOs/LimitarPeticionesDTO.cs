using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class LimitarPeticionesDTO
    {
        public const string Seccion = "limitarPeticiones";
        [Required]
        public int PeticionesPorDiaGratuitos { get; set; }
    }
}
