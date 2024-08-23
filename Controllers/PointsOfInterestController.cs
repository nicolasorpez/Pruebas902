using AutoMapper;
using IDGS902UT.API.Entities;
using IDGS902UT.API.Models;
using IDGS902UT.API.Services;
using IDGS902UT_API;
using Microsoft.AspNetCore.Mvc;

namespace IDGS902UT.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityid}/pointsofinterest")]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ICityInfoRepository _repository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ICityInfoRepository repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult>GetPointsofInterestByCityId(int cityid)
        {

            if (! await _repository.ExistCityAsync(cityid))
            {
                return NotFound("No existe la ciudad en la BD");
            }

            var pois = await _repository.GetPointsOfInterestsAsync(cityid);
            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pois));

        }

        [HttpGet("{poid}", Name = "GetPointOfInterest")]

        public IActionResult GetPuntodeInteresById(int cityid, int poid)
        {
            var city = CitiesDataStore.Current.Cities?.FirstOrDefault(c => c.Id.Equals(cityid));
            if (city == null)
            {
                // Si no se encuentra la ciudad, retorna un mensaje de error
                return NotFound($"No existe ninguna ciudad con el id {cityid} en la BD");
            }

            // Busca el punto de interés dentro de la ciudad encontrada
            var point = city.PointsOfInterest.FirstOrDefault(p => p.Id.Equals(poid));
            if (point == null)
            {
                // Si no se encuentra el punto de interés, retorna un mensaje de error
                return NotFound($"No existe ningun punto de interes con el id {poid}, en {city.Name}");
            }

            // Crea un DTO (Data Transfer Object) para el punto de interés encontrado
            var pointDto = new PointOfInterestDto
            {
                Id = point.Id,
                Name = point.Name,
                Description = point.Description
            };

            // Retorna el DTO del punto de interés encontrado
            return Ok(pointDto);
        }



        [HttpPost]
        public async Task<IActionResult> CreatePointOfInterest(int cityid, [FromBody] PointOfInterestForCreationDto nuevopoi)

        {    //buscar la ciudad y si no se encuentra mandar un error 404
            var city = await _repository.GetCityByIdAsync(cityid, true);
            if (city == null)
            {
                return NotFound($"No existe una ciudad con el Id {cityid} en la base de datos");
            }
            else
            {

                //crear un punto de interes Dto
                var finalpoi = _mapper.Map<PointOfInterest>(nuevopoi);

                //agregarlo al almacen de datos de la ciudad
              await _repository.AddPointOfInterestAsync(cityid,finalpoi);

               var exito = await _repository.SaveChangesToDBAsync();
                if(!exito)
                {
                    return BadRequest("Error al guardar la info en la BD!!");
                }
                //si inserto en la BD tenemos que regresar el nuevopoi en unDTO

                var poiCreatedInBD = _mapper.Map<PointOfInterestDto>(finalpoi);
                
                //regresar el status Code adecuado y la URL al nuevo recurso
                return CreatedAtRoute("GetPointOfInterest", new
                {
                    cityid = cityid,
                    poid = finalpoi.Id,
                }, poiCreatedInBD);

            }


        }

        [HttpPut("{poid}")]
        public async Task<IActionResult> UpdatePointOfInterest(int cityid, int poid, [FromBody] PointOfInterestForUpdateDto poi)
        {
            var city = await _repository.GetCityByIdAsync(cityid, true);
            if (city == null)
            {
                return NotFound($"No existe una ciudad con el Id {cityid} en la base de datos");
            }

            var poidb =await _repository.GetPointOfInterestAsync(cityid, poid);  

            if (poidb is null)
            {
                return NotFound();
            }

            _mapper.Map(poi, poidb);

            await _repository.SaveChangesToDBAsync();

            return NoContent();
           
        }

        [HttpDelete("{poid}")]
        public async Task< IActionResult> DeletePointOfInterest(int cityid, int poid)
        {
            var city = await _repository.GetCityByIdAsync(cityid, true);
            if (city == null)
            {
                return NotFound($"No existe una ciudad con el Id {cityid} en la base de datos");
            }
            var point = await _repository.GetPointOfInterestAsync(cityid, poid);

            if (point is null)
            {
                return NotFound();
            }

            _repository.DeletePointOfInterest(point);

           await _repository.SaveChangesToDBAsync();

                return NoContent();

            }

        }

      




     

}
