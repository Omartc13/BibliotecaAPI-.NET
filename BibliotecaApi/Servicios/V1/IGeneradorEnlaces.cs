using BibliotecaApi.DTOs;

namespace BibliotecaApi.Servicios.V1
{
    public interface IGeneradorEnlaces
    {
        Task GenerarEnlaces(AutorDTO autorDTO);
        Task<ColeccionDeRecursosDTO<AutorDTO>> GenerarEnlaces(List<AutorDTO> autores);
    }
}