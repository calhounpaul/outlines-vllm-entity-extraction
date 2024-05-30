using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using CodedenimWebApp.Models;
using CodedenimWebApp.Service;
using CodedenimWebApp.ViewModels;
using CodeninModel.Quiz;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;

namespace CodedenimWebApp.Controllers.Api
{
    public class TakeQuizApiController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly StartQuiz _startQuiz  = new StartQuiz();
        public async Task<IHttpActionResult> ShowNumbers()
        {
            var studentName =  User.Identity.GetUserName();
            var questionNumber = await _db.StudentTopicQuizs.AsNoTracking().Where(x => x.StudentId.Equals(studentName))
                .OrderBy(o => o.QuestionNumber).ToListAsync();
            return Ok(questionNumber);
        }
        public IHttpActionResult Menu(string studentId, int ModuleId)
        {
            var questionNumber = _db.StudentTopicQuizs.AsNoTracking().Where(x => x.StudentId.Equals(studentId)
                                                                                 && x.ModuleId.Equals(ModuleId))
                .OrderBy(o => o.QuestionNumber);

            return Ok(questionNumber);
        }


        // [Authorize(Roles = RoleName.Student)]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetSelectSubject(int ModuleId, string email)
        {
            if (ModelState.IsValid)
            {
                var convert = new ConvertEmail();
                var studentemail = convert.ConvertEmailToId(email);
                var studentId = studentemail;

                var questionExist =  _db.StudentTopicQuizs.AsNoTracking().Count(x => x.StudentId.Equals(studentId)
                                                                                          && x.ModuleId.Equals(ModuleId));
                var questionNo = 1;
                
                if (questionExist > 1)
                {
                    return Exam(questionNo, ModuleId, studentId);
                    //return RedirectToRoute("Exam", new
                    //{
                    //    questionNo = 1,
                    //    topicId = topicId,
                    //    studentid = studentName
                    //});
                }

                //var r = new Random();

                Random rnd = new Random();
                var myquestion = _db.TopicQuizs.AsNoTracking().Where(x => x.ModuleId.Equals(ModuleId))
                    .OrderBy(x => Guid.NewGuid()).Take(2)
                    .DistinctBy(d => d.TopicQuizId).ToList();
                // var myquestion = bquestion.OrderBy(x => Guid.NewGuid()).Take(totalQuestion).ToList();
                //var tenRandomUser = listUsr.OrderBy(u => r.Next()).Take(10);

                int count = 1;
                foreach (var question in myquestion)
                {
                    var studentQuestion = new StudentTopicQuiz()
                    {
                        StudentId = studentId,
                        ModuleId = question.ModuleId,
                        Question = question.Question,
                        Option1 = question.Option1,
                        Option2 = question.Option2,
                        Option3 = question.Option3,
                        Option4 = question.Option4,
                        FilledAnswer = String.Empty,
                        Answer = question.Answer,
                        QuestionHint = question.QuestionHint,
                        IsFillInTheGag = question.IsFillInTheGag,
                        IsMultiChoiceAnswer = question.IsMultiChoiceAnswer,
                        QuestionNumber = count,
                        TotalQuestion = 2,
                        ExamTime = 5

                    };
                    _db.StudentTopicQuizs.Add(studentQuestion);
                    count++;
                }


                 _db.SaveChangesAsync();
                return Exam(questionNo,ModuleId, studentId);
                //return RedirectToRoute("Exam", new
                //{
                //    questionNo = 1,
                //    topicId = topicId,
                //    studentid = studentName,    
                //});
            }
            var date = DateTime.Now;

            return Ok();
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult Exam(int questionNo, int ModuleId, string studentid)
        {
            int myno = questionNo;
            var question =  _db.StudentTopicQuizs.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId.Equals(studentid)
                                          && s.ModuleId.Equals(ModuleId) && s.QuestionNumber.Equals(myno));
            //if (question != null)
            //{
            //    if (Session["Rem_Time"] == null)
            //    {
            //        int time = 5;
            //        Session["Rem_Time"] = DateTime.Now.AddMinutes(time).ToString("MM-dd-yyyy h:mm:ss tt");
            //        //Session["Rem_Time"] = DateTime.Now.AddMinutes(1).ToString("MM-dd-yyyy h:mm:ss tt");
            //    }
            //    //Session["Rem_Time"] = DateTime.Now.AddMinutes(2).ToString("dd-MM-yyyy h:mm:ss tt");
            //    // Session["Rem_Time"] = DateTime.Now.AddMinutes(2).ToString("MM-dd-yyyy h:mm:ss tt");
            //    ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            //    ViewBag.Rem_Time = Session["Rem_Time"];
            //    //ViewBag.Course = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(courseId))
            //    //    .Select(c => c.CourseName).FirstOrDefaultAsync();

            //}
            return Ok(question);
        }
        public async Task<IHttpActionResult> ExamIndex(string studentId, int? topicId, string score)
        {
            var examInfo = new ExamIndexVm()
            {
                StudentId = studentId,
                TopicId = topicId,
                Score = score
            };
            //ViewBag.StudentId = studentId;
            //ViewBag.topicId = topicId;
            //ViewBag.Score = score;

           /// Session["Rem_Time"] = null;
            return Ok(examInfo);
            //return View(studentList.ToList());
        }

    }
}

