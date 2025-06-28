using System;

namespace AP_Project.Data
{
    public static class CodeGenerator
    {
        public static int GenerateInstructorCode(int year) => GeneratePersonCode(false, year);
        public static int GenerateStudentCode(int year) => GeneratePersonCode(true, year);

        private static int GeneratePersonCode(bool isStudent, int year)
        {
            var rand = new Random();

            int prefix = int.Parse($"{year.ToString().Substring(1,3)}");
            int mid = isStudent
                ? rand.Next(0, 5)
                : rand.Next(5, 10);
            int suffix = rand.Next(0, 100_000);

            return int.Parse($"{prefix:D3}{mid}{suffix:D5}");
        }
    }


}