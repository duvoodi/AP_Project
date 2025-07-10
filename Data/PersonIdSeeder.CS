using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AP_Project.Data;
public class CodeGenerator
{
    private readonly AppDbContext _context;
    private readonly Random _rand = new Random();

    public CodeGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GenerateInstructorCodeAsync(int year)
    {
        return await GenerateUniqueCodeAsync(false, year);
    }

    public async Task<int> GenerateStudentCodeAsync(int year)
    {
        return await GenerateUniqueCodeAsync(true, year);
    }

    private async Task<int> GenerateUniqueCodeAsync(bool isStudent, int year)
    {
        int code;
        bool exists;

        do
        {
            code = GeneratePersonCode(isStudent, year);

            // اینجا چک می‌کنیم کد قبلاً تو دیتابیس هست یا نه
            if (isStudent)
            {
                exists = await _context.Students.AnyAsync(s => s.StudentId == code);
            }
            else
            {
                exists = await _context.Instructors.AnyAsync(i => i.InstructorId == code);
            }

        } while (exists);

        return code;
    }

    private int GeneratePersonCode(bool isStudent, int year)
    {
        int prefix = int.Parse($"{year.ToString().Substring(1, 3)}");
        int mid = isStudent
            ? _rand.Next(0, 5)
            : _rand.Next(5, 10);
        int suffix = _rand.Next(0, 100_000);

        return int.Parse($"{prefix:D3}{mid}{suffix:D5}");
    }
}