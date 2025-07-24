using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AP_Project.Controllers
{
    public class FullInformationController : BaseStudentController
    {
        public FullInformationController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public async Task<IActionResult>  Index(string h)
        {
            var student = CurrentStudent;

            var takes = await _db.Takes
                .Where(t => t.StudentUserId == student.Id)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseCode)
                .Include(t => t.Section)
                    .ThenInclude(s => s.TimeSlot)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Classroom)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(te => te.Instructor)
                .ToListAsync();

            // فیلتر حذف هر رکوردی که سکشنش نال باشد
            takes = takes
                .Where(t => t.Section != null)
                .ToList();

            // گروه بندی بر اساس ترم
            var termGroups = takes
                .Where(t => t.Section != null)
                .GroupBy(t => new { t.Section.Year, t.Section.Semester })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Semester)
                .ToList();

            var termSummaries = new List<object>();

            int totalPassedTerms = 0;
            int totalProbations = 0;
            int totalPassedUnits = 0;
            double weightedSumAllTerms = 0;
            double totalUnitsWithGrade = 0;

            foreach (var group in termGroups)
            {
                var totalUnits = group.Sum(t => t.Section?.Course?.Unit ?? 0);
                var passedUnits = group.Where(t => double.TryParse(t.Grade, out var g) && g >= 10)
                                       .Sum(t => t.Section?.Course?.Unit ?? 0);
                var unitsWithoutGrade = group.Where(t => string.IsNullOrWhiteSpace(t.Grade))
                                            .Sum(t => t.Section?.Course?.Unit ?? 0);

                double weightedSum = 0;
                double gradedUnits = 0;
                foreach (var take in group)
                {
                    if (double.TryParse(take.Grade, out var g))
                    {
                        var unit = take.Section?.Course?.Unit ?? 0;
                        weightedSum += g * unit;
                        gradedUnits += unit;
                    }
                }
                var termAverage = gradedUnits > 0 ? Math.Round(weightedSum / gradedUnits, 2) : 0;

                if (passedUnits > 0)
                {
                    totalPassedTerms++;
                }
                if (termAverage > 0)
                {
                    weightedSumAllTerms += weightedSum;
                    totalUnitsWithGrade += gradedUnits;
                }
                if (termAverage < 12 && termAverage > 0)
                {
                    totalProbations++;
                }
                totalPassedUnits += passedUnits;

                termSummaries.Add(new
                {
                    Year = group.Key.Year,
                    Semester = group.Key.Semester,
                    TotalUnits = totalUnits,
                    PassedUnits = passedUnits,
                    UnitsWithoutGrade = unitsWithoutGrade,
                    TermAverage = termAverage
                });
            }

            double cumulativeAverage = totalUnitsWithGrade > 0 ? Math.Round(weightedSumAllTerms / totalUnitsWithGrade, 2) : 0;

            ViewBag.Student = student;
            ViewBag.Takes = takes;
            ViewBag.TermSummaries = termSummaries;
            ViewBag.PassedUnits = totalPassedUnits;
            ViewBag.PassedTerms = totalPassedTerms;
            ViewBag.CumulativeAverage = cumulativeAverage > 0 ? cumulativeAverage.ToString("0.00") : "-";
            ViewBag.TotalProbations = totalProbations;

            return View("~/Views/StudentDashboard/FullInformation.cshtml", student);
        }


    }
}