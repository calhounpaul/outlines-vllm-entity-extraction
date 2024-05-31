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
                }


                Random rnd = new Random();
                var myquestion = _db.TopicQuizs.AsNoTracking().Where(x => x.ModuleId.Equals(ModuleId))
                    .OrderBy(x => Guid.NewGuid()).Take(2)
                    .DistinctBy(d => d.TopicQuizId).ToList();

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
            return Ok(examInfo);
        }

    }
}

