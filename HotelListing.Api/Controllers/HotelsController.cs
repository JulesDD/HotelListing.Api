using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new Hotel{Id = 1, Name = "Grand Plaza", Address = "Over here", Rating = 4.8 },
            new Hotel{Id = 2, Name = "Ocean View", Address = "Over there", Rating = 4.8 }
        };
        // GET: api/<HotelsController>
        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            if (hotels.Count == 0)
            {
                return BadRequest("No hotels found in Database");
            }
            return Ok(hotels);
        }

        // GET api/<HotelsController>/5
        [HttpGet("{id}")]
        public ActionResult<Hotel> Get(int id) 
        {
            //Search for Id in listing
            var hotel = hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }

        // POST api/<HotelsController>
        [HttpPost]
        public ActionResult<Hotel> Post([FromBody] Hotel newHotel)
        {
            if(hotels.Any(h => h.Id == newHotel.Id))
            {
                return BadRequest("A hotel with this ID already exists");
            }
            hotels.Add(newHotel);
            return CreatedAtAction(nameof(Get), new { id = newHotel.Id } ,newHotel);
        }

        // PUT api/<HotelsController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Hotel updatedHotel)
        {
            var existingHotel = hotels.FirstOrDefault(h =>h.Id == id);
            if (existingHotel == null)
            {
                return NotFound(new { message = "Hotel not found" });
            }
            existingHotel.Id = id;
            existingHotel.Address = updatedHotel.Address;
            existingHotel.Rating = updatedHotel.Rating;

            return NoContent();
        }

        // DELETE api/<HotelsController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound(new { message = "Hotel not found" });
            }
            hotels.Remove(hotel);
            return NoContent();

        }
    }
}
