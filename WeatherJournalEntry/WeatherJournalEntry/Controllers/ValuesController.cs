using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherJournalEntry.Data;
using WeatherJournalEntry.Model;

namespace WeatherJournalEntry.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
        private readonly DataContext dataContext;

        public ValuesController(DataContext dataContext) {
            this.dataContext = dataContext;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get() {
            // test
            dataContext.Database.EnsureCreated();
            // add
            dataContext.Weathers.Add(new Weather {
                Id = 1,
                Main = "main",
                Description = "description",
                Icon = "icon"
            } );
            dataContext.SaveChanges();

            return new string[] { "value1", "value2", "testvalue" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id) {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value) {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }
    }
}
