using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FlexWork.Models;
using FlexWorkApp.Controllers;
using Microsoft.AspNet.Identity;

namespace FlexWork.Controllers
{
    [RoutePrefix("ratings")]
    public class RatingsController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PeformRating(Rating rating)
        {
            if (ModelState.IsValid == false)
                return BadRequest(ModelState);

            rating.UserId = User.Identity.GetUserId();
            rating.Timestamp = DateTime.Now;

            Context.Ratings.Add(rating);

            await Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("product/{id}")]
        public IHttpActionResult GetRatingsForWorkspace(Guid id)
        {
            return Ok(Context.Ratings.Include(r => r.User).Where(r => r.WorkspaceId == id));
        }

        [HttpGet]
        [Route("me/product/{id}")]
        public IHttpActionResult GetMyRatingForWorkspace(Guid id)
        {
            var userId = User.Identity.GetUserId();

            return Ok(Context.Ratings.SingleOrDefault(r => r.WorkspaceId == id && r.UserId == userId));
        }
    }
}
