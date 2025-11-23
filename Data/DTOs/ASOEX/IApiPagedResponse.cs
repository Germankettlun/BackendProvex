using System.Collections.Generic;

// Reemplaza este namespace con la ubicación de tus DTOs.
namespace ProvexApi.Data.DTOs.ASOEX
{
    /// <summary>
    /// Define el contrato para una respuesta de API paginada.
    /// </summary>
    /// <typeparam name="T">El tipo de los datos contenidos en la página.</typeparam>
    public interface IApiPagedResponse<T>
    {
        int current_page { get; set; }
        int last_page { get; set; }
        List<T> data { get; set; }
    }
}