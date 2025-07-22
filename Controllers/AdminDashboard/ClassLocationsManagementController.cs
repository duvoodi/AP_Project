using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AP_Project.FormViewModels.ClassForm;
using AP_Project.Models.Courses;
using AP_Project.Models.Classrooms;

namespace AP_Project.Controllers
{
    public class ClassLocationsManagementController : BaseAdminController
    {
        public ClassLocationsManagementController (AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult ManageClassLocationsIndex()
        {
            var admin = CurrentAdmin;
            ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
            return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClassLocation(AddClassLocationFormViewModel model)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(model);

            foreach (var prop in typeof(AddClassLocationFormViewModel).GetProperties())
            {
                ModelState.ValidateField(model, prop.Name, true);
            }

            if (
                ModelState.TryGetValue("AddBuilding", out var buildingState) && buildingState.Errors.Count == 0 &&
                ModelState.TryGetValue("AddRoomNumber", out var roomState) && roomState.Errors.Count == 0
            )
            {
                if (int.TryParse(model.AddRoomNumber, out int roomNum))
                {
                    bool exists = await _db.Classrooms.AnyAsync(c =>
                        c.Building == model.AddBuilding && c.RoomNumber == roomNum);
                    if (exists)
                    {
                        ModelState.AppendModelError("GeneralError", "این ساختمان و شماره کلاس قبلاً ثبت شده است.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["AddForm"] = model;
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }

            try
            {
                var classroom = new Classroom
                {
                    Building = model.AddBuilding,
                    RoomNumber = int.Parse(model.AddRoomNumber),
                    Capacity = int.Parse(model.AddCapacity)
                };
                _db.Classrooms.Add(classroom);
                await _db.SaveChangesAsync();

                return RedirectToAction("ManageClassLocationsIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.ReplaceModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["AddForm"] = model;
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassLocation(EditClassLocationFormViewModel model)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(model);

            foreach (var prop in typeof(EditClassLocationFormViewModel).GetProperties())
            {
                ModelState.ValidateField(model, prop.Name, true);
            }

            if (
                ModelState.TryGetValue("EditBuilding", out var buildingState) && buildingState.Errors.Count == 0 &&
                ModelState.TryGetValue("EditRoomNumber", out var roomState) && roomState.Errors.Count == 0
            )
            {
                if (int.TryParse(model.EditRoomNumber, out int roomNum))
                {
                    bool duplicate = await _db.Classrooms.AnyAsync(c =>
                        c.Building == model.EditBuilding &&
                        c.RoomNumber == roomNum &&
                        c.Id != model.EditId); // با خودش تکراری نگیرد
                    if (duplicate)
                    {
                        ModelState.AppendModelError("GeneralError", "این ساختمان و شماره کلاس قبلاً ثبت شده است.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["EditForm"] = model;
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }

            try
            {
                var classroom = await _db.Classrooms.FindAsync(model.EditId);
                if (classroom == null)
                {
                    ModelState.ReplaceModelError("GeneralError", "کلاس یافت نشد!");
                    ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                    ViewData["EditForm"] = model;
                    return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
                }

                classroom.Building = model.EditBuilding;
                classroom.RoomNumber = int.Parse(model.EditRoomNumber);
                classroom.Capacity = int.Parse(model.EditCapacity);
                await _db.SaveChangesAsync();

                return RedirectToAction("ManageClassLocationsIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.ReplaceModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["EditForm"] = model;
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassLocation(Guid DeleteId)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");

            if (DeleteId == Guid.Empty)
            {
                ModelState.AppendModelError("DeleteId", "لطفاً یک مورد را انتخاب کنید");
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }

            try
            {
                var classroom = await _db.Classrooms.FindAsync(DeleteId);
                if (classroom == null)
                {
                    ModelState.ReplaceModelError("GeneralError", "کلاس یافت نشد!");
                    ViewData["DeleteSelect"] = DeleteId;
                    ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                    return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
                }

                bool isUsed = await _db.Sections.AnyAsync(s => s.ClassroomId == DeleteId);
                if (isUsed)
                {

                    ModelState.ReplaceModelError("GeneralError", "این کلاس هم اکنون در حال استفاده است و نمی‌توان آن را حذف کرد!");
                    ViewData["DeleteSelect"] = DeleteId;
                    ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                    return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
                }

                _db.Classrooms.Remove(classroom);
                await _db.SaveChangesAsync();

                return RedirectToAction("ManageClassLocationsIndex", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.ReplaceModelError("GeneralError", "خطایی هنگام حذف اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["DeleteSelect"] = DeleteId;
                ViewData["Classrooms"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                return View("~/Views/AdminDashboard/ClassManagement/ManageClassLocations.cshtml", admin);
            }
        }
    }
}

