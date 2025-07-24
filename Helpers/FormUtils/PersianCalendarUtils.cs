using System;
using System.Globalization;

namespace AP_Project.Helpers
{
    public static class PersianCalendarUtils
    {
        public static bool IsValidPersianDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date) || date.Length != 10)
                return false;

            try
            {
                var parts = date.Split('/');
                if (parts.Length != 3) return false;

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                var pc = new PersianCalendar();
                DateTime dt = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidSemesterDate(string semester, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(semester) || semester.Length != 5 || !int.TryParse(semester, out _))
            {
                errorMessage = "نیم‌سال باید عددی 5 رقمی باشد";
                return false;
            }

            var yearPart = int.Parse(semester.Substring(0, 4));
            var semesterPart = int.Parse(semester[4].ToString());

            if (yearPart < 1300)
            {
                errorMessage = "سال ترم باید از 1300 به بعد باشد";
                return false;
            }

            if (semesterPart < 1 || semesterPart > 3)
            {
                errorMessage = "بخش ترم باید بین 1 تا 3 باشد";
                return false;
            }

            return true;
        }

        public static bool IsValidExamDateWithSemester(string examDate, string semester, out string errorMessage)
        {
            errorMessage = null;

            if (!IsValidPersianDate(examDate))
            {
                errorMessage = "تاریخ نامعتبر است";
                return false;
            }

            var parts = examDate.Split('/');
            int examYear = int.Parse(parts[0]);
            int examMonth = int.Parse(parts[1]);


            if (string.IsNullOrWhiteSpace(semester) || semester.Length != 5 || !int.TryParse(semester, out _))
            {
                return true;
            }

            var yearPart = int.Parse(semester.Substring(0, 4));
            var semesterPart = int.Parse(semester[4].ToString());

            if (yearPart < 1300)
            {
                return true;
            }

            if (semesterPart < 1 || semesterPart > 3)
            {
                return true;
            }

            if ((semesterPart == 1 && examYear != yearPart) || (semesterPart != 1 && examYear != yearPart + 1))
            {
                errorMessage = "سال تاریخ امتحان با ترم همخوانی ندارد";
                return false;
            }

            bool monthMatchesSemester = semesterPart switch
            {
                1 => examMonth == 10 || examMonth == 11,
                2 => examMonth == 3 || examMonth == 4,
                3 => examMonth == 5 || examMonth == 6,
                _ => false
            };

            if (!monthMatchesSemester)
            {
                errorMessage = "ماه تاریخ امتحان با ترم همخوانی ندارد";
                return false;
            }

            return true;
        }

    }
}
