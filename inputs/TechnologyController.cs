using LiveHackDb.Models;
using LiveHack.Models.ViewModels;
using LiveHack.Models.BindingModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using LiveHack.Hubs;

namespace LiveHack.Controllers
{
    [RoutePrefix("api/Technology")]
    [Authorize]
    public class TechnologyController : ApiController
    {
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetTechnologies()
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = IdentityExtensions.GetUserId(RequestContext.Principal.Identity);
                var user = db.Users.SingleOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                var technologies = db.Chats.Include("Users.ChatsOwned").Include("Owners.ChatsOwned").OfType<Technology>().OrderBy(a => a.Name).ToList().Select(a => a.ToViewModel());
                return Ok(technologies);
            }
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> PostCreate(TechnologyBindingModel inputTech)
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = IdentityExtensions.GetUserId(RequestContext.Principal.Identity);
                var user = db.Users.SingleOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    return Unauthorized();
                }
                if (db.Chats.OfType<Technology>().SingleOrDefault(t => t.Name.ToLower() == inputTech.Name.ToLower()) != null)
                {
                    return BadRequest("A technology with this name already exists.");
                }

                var technology = new Technology()
                {
                    Id = Guid.NewGuid(),
                    Name = inputTech.Name,
                    Description = "",
                    DateTimeCreated = DateTimeOffset.Now,
                    Messages = new List<Message>(),
                    Users = new List<User>() { user },
                    Owners = new List<User>()
                };
                db.Chats.Add(technology);
                await db.SaveChangesAsync();
                LiveHackHub.AddAllToGroup(technology.Id.ToString());
                LiveHackHub.SendNewTechnology(technology);
                return Created("/api/Technology/" + technology.Id, technology.ToViewModel());
            }
        }
    }
}
