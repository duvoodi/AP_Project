using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Models.Courses;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AP_Project.Controllers
{
    public class CourseCodesManagementController : BaseAdminController
    {
        public CourseCodesManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult ManageCourseCodesIndex()
        {
            var admin = CurrentAdmin;
            ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
            return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseCode(AddCourseCodeFormViewModel model)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(model);

            foreach (var prop in typeof(AddCourseCodeFormViewModel).GetProperties())
            {
                ModelState.ValidateField(model, prop.Name, true);
            }

            if (ModelState.TryGetValue("AddCode", out var addCodeState) && addCodeState.Errors.Count == 0)
            {
                bool codeExists = await _db.CourseCodes.AnyAsync(c => c.Code.ToString() == model.AddCode);
                if (codeExists)
                {
                    ModelState.AppendModelError("AddCode", "این کد درس قبلاً ثبت شده است.");
                }
            }

            if (ModelState.TryGetValue("AddTitle", out var addTitleState) && addTitleState.Errors.Count == 0)
            {
                bool codeExists = await _db.CourseCodes.AnyAsync(c => c.Code.ToString() == model.AddTitle);
                if (codeExists)
                {
                    ModelState.AppendModelError("AddTitle", "این عنوان درس قبلاً ثبت شده است.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["AddForm"] = model;
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }

            try
            {
                var courseCode = new CourseCode
                {
                    Code = int.Parse(model.AddCode),
                    Title = model.AddTitle
                };
                _db.CourseCodes.Add(courseCode);

                await _db.SaveChangesAsync();
                return RedirectToAction("ManageCourseCodesIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.ReplaceModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["AddForm"] = model;
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourseCode(EditCourseCodeFormViewModel model)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(model);

            foreach (var prop in typeof(EditCourseCodeFormViewModel).GetProperties())
            {
                ModelState.ValidateField(model, prop.Name, true);
            }

            if (ModelState.TryGetValue("EditCode", out var editCodeState) && editCodeState.Errors.Count == 0)
            {
                bool duplicate = await _db.CourseCodes
                    .AnyAsync(c => c.Code.ToString() == model.EditCode && c.Id != model.EditId); // با خودش تکراری نمیگیره

                if (duplicate)
                {
                    ModelState.AppendModelError("EditCode", "این کد درس قبلاً ثبت شده است.");
                }
            }

            if (ModelState.TryGetValue("EditTitle", out var addTitleState) && addTitleState.Errors.Count == 0)
            {
                bool codeExists = await _db.CourseCodes.AnyAsync(c => c.Code.ToString() == model.EditTitle);
                if (codeExists)
                {
                    ModelState.AppendModelError("EditTitle", "این عنوان درس قبلاً ثبت شده است.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["EditForm"] = model;
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }

            try
            {
                var courseCode = await _db.CourseCodes.FindAsync(model.EditId);
                if (courseCode == null)
                {
                    ModelState.ReplaceModelError("GeneralError", "کد درس یافت نشد!");
                    ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                    ViewData["EditForm"] = model;
                    return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
                }

                courseCode.Code = int.Parse(model.EditCode);
                courseCode.Title = model.EditTitle;

                await _db.SaveChangesAsync();
                return RedirectToAction("ManageCourseCodesIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["EditForm"] = model;
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseCode(Guid DeleteId)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");

            if (DeleteId == Guid.Empty)
            {
                ModelState.AppendModelError("DeleteId", "لطفاً یک مورد را انتخاب کنید");
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }

            try
            {
                var courseCode = await _db.CourseCodes.FindAsync(DeleteId);
                if (courseCode == null)
                {
                    ModelState.ReplaceModelError("GeneralError", "کد درس یافت نشد!");
                    ViewData["DeleteSelect"] = DeleteId;
                    ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                    return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
                }

                bool isUsed = await _db.Courses.AnyAsync(c => c.CodeId == DeleteId);
                if (isUsed)
                {
                    ModelState.ReplaceModelError("GeneralError", "این کد درس هم اکنون در حال استفاده است و نمی‌توان آن را حذف کرد!");
                    ViewData["DeleteSelect"] = DeleteId;
                    ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                    return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
                }

                _db.CourseCodes.Remove(courseCode);

                await _db.SaveChangesAsync();
                return RedirectToAction("ManageCourseCodesIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.ReplaceModelError("GeneralError", "خطایی هنگام حذف اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["DeleteSelect"] = DeleteId;
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                return View("~/Views/AdminDashboard/CourseManagement/ManageCourseCodes.cshtml", admin);
            }
        }
    }
}