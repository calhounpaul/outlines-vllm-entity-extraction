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


        //[System.Web.Http.HttpPost]
        //[ValidateInput(false)]
        //[MultipleButton(Name = "action", Argument = "Next")]
        //public async Task<IHttpActionResult> Next(DisplayQuestionViewModel model, string fiiledAnswer,
        //    string Check1, string Check2, string Check3, string Check4)
        //{
        //    var studentId = model.StudentId;
        //    int questionId = model.QuestionNo;
        //    var nextQuestion = 0;
        //    if (model.NextQuestion != null)
        //    {
        //        nextQuestion = (int)model.NextQuestion;
        //    }
        //    else
        //    {
        //        nextQuestion = questionId + 1;
        //    }

        //    var questionType = _startQuiz.CheckQuestionType(model);
        //    if (questionType != null)
        //    {

        //        if (questionType.IsFillInTheGag)
        //        {
        //            if (!String.IsNullOrEmpty(fiiledAnswer))
        //            {
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, fiiledAnswer);
        //                return RedirecToAction("Exam", "TakeQuiz",
        //                    new
        //                    {
        //                        questionNo = ++questionId,
        //                        topicId = model.TopicId,
        //                        studentid = model.StudentId

        //                    });
        //            }
        //        }
        //        else if (questionType.IsMultiChoiceAnswer)
        //        {
        //            StringBuilder builder = new StringBuilder();

        //            if (model.Check1.Equals(true))
        //            {
        //                builder.Append("A");
        //            }
        //            if (model.Check2.Equals(true))
        //            {
        //                builder.Append("B");
        //            }
        //            if (model.Check3.Equals(true))
        //            {
        //                builder.Append("C");
        //            }
        //            if (model.Check4.Equals(true))
        //            {
        //                builder.Append("D");
        //            }
        //            string value = builder.ToString();

        //            string checkedAnswer = _startQuiz.SortStringAlphabetically(builder.ToString());

        //            await _startQuiz.SaveMultiChoiceAnswer(model, checkedAnswer);
        //        }
        //        else
        //        {
        //            if (!String.IsNullOrEmpty(model.SelectedAnswer))
        //            {
        //                string answer = _startQuiz.CheckAnswerForSingleChoice(model);
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, answer);

        //                return RedirectToAction("Exam", "TakeQuiz",
        //                    new
        //                    {
        //                        questionNo = nextQuestion,
        //                        topicId = model.TopicId,
        //                        studentid = model.StudentId

        //                    });
        //            }
        //        }
        //    }

        //    ViewBag. = new SelectList(_db.Courses, "CourseId", "CourseCode");

        //    return RedirectToRoute("Exam", new { questionNo = nextQuestion, topicId = model.TopicId, studentid = model.StudentId });
        //}




        //[System.Web.Http.HttpPost]
        //[ValidateInput(false)]
        //[MultipleButton(Name = "action", Argument = "Previous")]
        //public async Task<IHttpActionResult> Previous(DisplayQuestionViewModel model, string fiiledAnswer,
        //    string Check1, string Check2, string Check3, string Check4)
        //{
        //    var studentId = model.StudentId;
        //    int questionId = model.QuestionNo;
        //    var questionType = _startQuiz.CheckQuestionType(model);
        //    if (questionType != null)
        //    {
        //        if (questionType.IsFillInTheGag)
        //        {
        //            if (!String.IsNullOrEmpty(fiiledAnswer))
        //            {
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, fiiledAnswer);
        //                return RedirecToAction("Exam", "TakeQuiz", new
        //                {
        //                    questionNo = ++questionId,
        //                    topicId = model.TopicId,
        //                    studentid = model.StudentId

        //                });
        //            }
        //        }
        //        else if (questionType.IsMultiChoiceAnswer)
        //        {
        //            StringBuilder builder = new StringBuilder();

        //            if (model.Check1.Equals(true))
        //            {
        //                builder.Append("A");
        //            }
        //            if (model.Check2.Equals(true))
        //            {
        //                builder.Append("B");
        //            }
        //            if (model.Check3.Equals(true))
        //            {
        //                builder.Append("C");
        //            }
        //            if (model.Check4.Equals(true))
        //            {
        //                builder.Append("D");
        //            }
        //            string value = builder.ToString();

        //            string checkedAnswer = _startQuiz.SortStringAlphabetically(builder.ToString());

        //            await _startQuiz.SaveMultiChoiceAnswer(model, checkedAnswer);
        //        }
        //        else
        //        {
        //            if (!String.IsNullOrEmpty(model.SelectedAnswer))
        //            {

        //                string answer = _startQuiz.CheckAnswerForSingleChoice(model);
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, answer);
        //                return RedirectToAction("Exam", "TakeQuiz",
        //                    new
        //                    {
        //                        questionNo = --questionId,
        //                        topicId = model.TopicId,
        //                        studentid = model.StudentId
        //                    });
        //            }
        //        }
        //    }

        //    ViewBag.SubjectName = new SelectList(_db.Courses, "CourseId", "CourseCode");
        //    return RedirectToRoute("Exam", new { questionNo = --questionId, topicId = model.TopicId, studentid = model.StudentId });
        //}


        //[System.Web.Http.HttpPost]
        //[ValidateInput(false)]
        //[MultipleButton(Name = "action", Argument = "SubmitExam")]
        //public async Task<IHttpActionResult> SubmitExam(DisplayQuestionViewModel model, string fiiledAnswer,
        //    string Check1, string Check2, string Check3, string Check4)
        //{
        //    double scoreCount = 0;

        //    var studentId = model.StudentId;
        //    int questionId = model.QuestionNo;

        //    var questionType = _startQuiz.CheckQuestionType(model);
        //    #region Submit Answer
        //    if (questionType != null)
        //    {
        //        if (questionType.IsFillInTheGag)
        //        {
        //            if (!String.IsNullOrEmpty(fiiledAnswer))
        //            {
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, fiiledAnswer);
        //                //scoreCount = _db.StudentTopicQuizes.Count(x => x.IsCorrect.Equals(true));
        //                //return RedirectToAction("ExamIndex", "TakeExam",
        //                //    new { score = scoreCount, courseId = model.CourseId, studentid = model.StudentId });
        //            }
        //        }
        //        else if (questionType.IsMultiChoiceAnswer)
        //        {
        //            StringBuilder builder = new StringBuilder();

        //            if (model.Check1.Equals(true))
        //            {
        //                builder.Append("A");
        //            }
        //            if (model.Check2.Equals(true))
        //            {
        //                builder.Append("B");
        //            }
        //            if (model.Check3.Equals(true))
        //            {
        //                builder.Append("C");
        //            }
        //            if (model.Check4.Equals(true))
        //            {
        //                builder.Append("D");
        //            }
        //            string value = builder.ToString();

        //            string checkedAnswer = _startQuiz.SortStringAlphabetically(builder.ToString());

        //            await _startQuiz.SaveMultiChoiceAnswer(model, checkedAnswer);
        //        }
        //        else
        //        {
        //            if (!String.IsNullOrEmpty(model.SelectedAnswer))
        //            {
        //                string answer = _startQuiz.CheckAnswerForSingleChoice(model);
        //                await _startQuiz.SaveAnswer(model, studentId, questionId, answer);
        //                scoreCount = _db.StudentTopicQuizs.Count(x => x.IsCorrect.Equals(true));
        //                //return RedirectToAction("ExamIndex", "TakeExam",
        //                //    new { score = scoreCount, courseId = model.CourseId, studentid = model.StudentId });
        //            }
        //        }

        //    }
        //    #endregion

        //    scoreCount = await _db.StudentTopicQuizs.AsNoTracking().Where(x => x.StudentId.Equals(model.StudentId)
        //                                                                       && x.TopicId.Equals(model.TopicId))
        //        .CountAsync(c => c.IsCorrect.Equals(true));

        //    var studentdetails = _db.StudentTopicQuizs.AsNoTracking().FirstOrDefault(x => x.StudentId.Equals(model.StudentId)
        //                                                                                  && x.TopicId.Equals(model.TopicId));

        //    if (studentdetails != null)
        //    {
        //        await ProcessResult(model.TopicId, studentdetails, scoreCount);
        //    }

        //    //return RedirectToAction("Index", "ExamLogs", new
        //    //{
        //    //    studentId = model.StudentId,
        //    //    courseId = model.CourseId,
        //    //    levelId = model.LevelId,
        //    //    semesterId = model.SemesterId,
        //    //    sessionId = model.SessionId
        //    //});
        //    var myMsg = "Exam Saved Successfully...";
        //    TempData["Message"] = myMsg;
        //    return RedirectToAction("LogOff", "Account", new { message = myMsg });
        //}

        //public IHttpActionResult FinishExam()
        //{
        //    return Ok();
        //}

        //private async Task ProcessResult(int topicId, StudentTopicQuiz studentdetails, double scoreCount)
        //{
        //    //var examRule = await _db.ExamRules.AsNoTracking().Where(x => x.CourseId.Equals(topicId))
        //    //    .Select(s => new { scorePerQuestion = s.ScorePerQuestion, totalQuestion = s.TotalQuestion })
        //    //    .FirstOrDefaultAsync();
        //    double sum = 1 * 2;
        //    double total = scoreCount * 1;
        //    var examLog = new QuizLog()
        //    {
        //        StudentId = studentdetails.StudentId,
        //        TopicId = studentdetails.TopicId,
        //        Score = total,
        //        TotalScore = sum,
        //        ExamTaken = true
        //    };
        //    // _db.ExamLogs.AddOrUpdate(examLog);
        //    _db.Set<QuizLog>().AddOrUpdate(examLog);
        //    await _db.SaveChangesAsync();

        //    Session["Rem_Time"] = null;

        //    #region send message after exam
        //    //string courseName = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(model.CourseId))
        //    //    .Select(c => c.CourseName).FirstOrDefaultAsync();
        //    //string examName = await _db.ExamTypes.AsNoTracking().Where(x => x.ExamTypeId.Equals(model.ExamTypeId))
        //    //    .Select(c => c.ExamName).FirstOrDefaultAsync();
        //    //var semesterName =
        //    //    await _db.Semesters.AsNoTracking()
        //    //        .Where(x => x.ActiveSemester.Equals(true))
        //    //        .Select(c => c.SemesterName)
        //    //        .FirstOrDefaultAsync();
        //    //var sessioName =
        //    //    await _db.Sessions.AsNoTracking()
        //    //        .Where(x => x.ActiveSession.Equals(true))
        //    //        .Select(c => c.SessionName)
        //    //        .FirstOrDefaultAsync();
        //    //var message = new SmsToStudent()
        //    //{
        //    //    Destination = model.StudentId,
        //    //    Body =
        //    //        $"Your score for {courseName} in {examName} for {semesterName} semester in {sessioName} is: {total}/{sum}"
        //    //};
        //    //CustomSms cs = new CustomSms();
        //    //await cs.SendStudentMsgAsync(message); 
        //    #endregion
        //}

        //private async Task ProcessResultTimeOut(QuizLogVm model, StudentTopicQuiz studentdetails, double scoreCount)
        //{
        //    //var examRule = await _db.ExamRules.AsNoTracking().Where(x => x.ResultDivision.Equals(model.ExamTypeId))
        //    //    .Select(s => new { scorePerQuestion = s.ScorePerQuestion, totalQuestion = s.TotalQuestion })
        //    //    .FirstOrDefaultAsync();
        //    //double sum = examRule.scorePerQuestion * examRule.totalQuestion;
        //    //double total = scoreCount * examRule.scorePerQuestion;
        //    //var examLog = new QuizLog()
        //    //{
        //    //    StudentId = studentdetails.StudentId,
        //    //    TopicId = studentdetails.TopicId,
        //    //    // LevelId = studentdetails.LevelId,

        //    //    Score = total,
        //    //    TotalScore = sum,
        //    //    ExamTaken = true
        //    //};

        //    // _db.ExamLogs.AddOrUpdate(examLog);
        //    //_db.Set<QuizLog>().AddOrUpdate(examLog);
        //    //await _db.SaveChangesAsync();

        //    //Session["Rem_Time"] = null;

        //    //#region Send message after exam
        //    //string courseName = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(model.CourseId))
        //    //    .Select(c => c.CourseName).FirstOrDefaultAsync();
        //    //string examName = await _db.ExamTypes.AsNoTracking().Where(x => x.ExamTypeId.Equals(model.ExamTypeId))
        //    //    .Select(c => c.ExamName).FirstOrDefaultAsync();
        //    //var semesterName =
        //    //    await _db.Semesters.AsNoTracking()
        //    //        .Where(x => x.ActiveSemester.Equals(true))
        //    //        .Select(c => c.SemesterName)
        //    //        .FirstOrDefaultAsync();
        //    //var sessioName =
        //    //    await _db.Sessions.AsNoTracking()
        //    //        .Where(x => x.ActiveSession.Equals(true))
        //    //        .Select(c => c.SessionName)
        //    //        .FirstOrDefaultAsync();
        //    //var message = new SmsToStudent()
        //    //{
        //    //    Destination = model.StudentId,
        //    //    Body =
        //    //        $"Your score for {courseName} in {examName} for {semesterName} in {sessioName} is: {total}/{sum}"
        //    //};
        //    //CustomSms cs = new CustomSms();
        //    //await cs.SendStudentMsgAsync(message); 
        //    // #endregion
        //}


        //public async Task<IHttpActionResult> SubmitExam(string studentId, int topicId, int examType)
        //{
        //    double scoreCount = 0;
        //    //string myStudentId = studentId.Trim();
        //    //var semesterId = await _db.Semesters.AsNoTracking()
        //    //    .Where(x => x.ActiveSemester.Equals(true))
        //    //    .Select(c => c.SemesterId)
        //    //    .FirstOrDefaultAsync();
        //    //var sessionId =
        //    //    await _db.Sessions.AsNoTracking()
        //    //        .Where(x => x.ActiveSession.Equals(true))
        //    //        .Select(c => c.SessionId)
        //    //        .FirstOrDefaultAsync();




        //    scoreCount = await _db.StudentTopicQuizs.AsNoTracking().Where(x => x.StudentId.Equals(studentId) && x.TopicId.Equals(topicId))
        //        .CountAsync(c => c.IsCorrect.Equals(true));
        //    var studentdetails = _db.StudentTopicQuizs.AsNoTracking().FirstOrDefault(x => x.StudentId.Equals(studentId)
        //                                                                                  && x.TopicId.Equals(topicId));

        //    if (studentdetails != null)
        //    {
        //        await ProcessResult(topicId, studentdetails, scoreCount);
        //    }


        //    //return RedirectToAction("Index", "ExamLogs", new
        //    //{
        //    //    studentId = studentId,
        //    //    courseId = courseId,
        //    //    levelId = levelId,
        //    //    semesterId = semesterId,
        //    //    sessionId = sessionid
        //    //});
        //    return RedirectToRoute("LogOff", "Account");
        //}

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

        //[ValidateInput(false)]
        //private async Task SaveAnswer(DisplayQuestionViewModel model, string studentId, int questionId, string answer)
        //{
        //    var question = await _db.StudentTopicQuizs.AsNoTracking().FirstOrDefaultAsync
        //    (s => s.StudentId.Equals(studentId) && s.TopicId.Equals(model.TopicId)
        //          && s.QuestionNumber.Equals(questionId));
        //    if (question.Answer.ToUpper().Equals(answer.ToUpper()))
        //    {
        //        question.IsCorrect = true;
        //        question.SelectedAnswer = model.SelectedAnswer;
        //        question.Check1 = model.Check1;
        //        question.Check2 = model.Check2;
        //        question.Check3 = model.Check3;
        //        question.Check4 = model.Check4;
        //        question.FilledAnswer = answer;
        //        _db.Set<StudentTopicQuiz>().AddOrUpdate(question);
        //        // _db.Entry(question).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        question.IsCorrect = false;
        //        question.SelectedAnswer = model.SelectedAnswer;
        //        question.Check1 = model.Check1;
        //        question.Check2 = model.Check2;
        //        question.Check3 = model.Check3;
        //        question.Check4 = model.Check4;
        //        question.FilledAnswer = answer;
        //        _db.Set<StudentTopicQuiz>().AddOrUpdate(question);
        //        //_db.Entry(question).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //    }
        //}

        //private async Task SaveMultiChoiceAnswer(DisplayQuestionViewModel model, string checkedAnswer)
        //{
        //    var question = _db.StudentTopicQuizs.AsNoTracking().FirstOrDefault(s => s.StudentId.Equals(model.StudentId)
        //                                                                             && s.QuestionNumber.Equals(model.QuestionNo)
        //                                                                             && s.TopicId.Equals(model.TopicId));
        //    string[] myAnswer = question.Answer.Split(',');
        //    StringBuilder answerbuilder = new StringBuilder();

        //    foreach (var item in myAnswer)
        //    {
        //        answerbuilder.Append(item);
        //    }

        //    string value = answerbuilder.ToString();
        //    string answer = SortStringAlphabetically(answerbuilder.ToString());


        //    if (answer.ToUpper().Equals(checkedAnswer.ToUpper()))
        //    {
        //        question.IsCorrect = true;
        //        question.SelectedAnswer = model.SelectedAnswer;
        //        question.Check1 = model.Check1;
        //        question.Check2 = model.Check2;
        //        question.Check3 = model.Check3;
        //        question.Check4 = model.Check4;
        //        _db.Entry(question).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        question.IsCorrect = false;
        //        question.SelectedAnswer = model.SelectedAnswer;
        //        question.Check1 = model.Check1;
        //        question.Check2 = model.Check2;
        //        question.Check3 = model.Check3;
        //        question.Check4 = model.Check4;
        //        _db.Entry(question).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //    }
        //}

        //static string SortStringAlphabetically(string str)
        //{
        //    char[] foo = str.ToArray();
        //    Array.Sort(foo);
        //    return new string(foo);
        //}

        //private string CheckAnswerForSingleChoice(DisplayQuestionViewModel model)
        //{
        //    if (model.SelectedAnswer.Equals(model.Option1))
        //    {
        //        return "A";
        //    }
        //    if (model.SelectedAnswer.Equals(model.Option2))
        //    {
        //        return "B";
        //    }
        //    if (model.SelectedAnswer.Equals(model.Option3))
        //    {
        //        return "C";
        //    }
        //    if (model.SelectedAnswer.Equals(model.Option4))
        //    {
        //        return "D";
        //    }
        //    return "";
        //}

        //private StudentTopicQuiz CheckQuestionType(DisplayQuestionViewModel model)
        //{
        //    var questionType = _db.StudentTopicQuizs.FirstOrDefault(x => x.QuestionNumber.Equals(model.QuestionNo)
        //                                                                  && x.StudentId.Equals(model.StudentId) &&
        //                                                                  x.TopicId.Equals(model.TopicId));
        //    return questionType;
        //}
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}

