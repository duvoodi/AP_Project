# Golestan Educational System

**Team Members:** Amir Mohammad Davoodi & Kousha Majlesi  
**AP Project – Software Engineering**  
A simplified academic management system inspired by the real-world *Golestan* platform used in Iranian universities.

---

## 🎯 Project Overview

The **Golestan Educational System** is a simplified web-based platform designed to manage various academic processes for students, instructors, and administrators. The system allows user authentication, course and class management, student enrollment, grade recording, and detailed role-based access.

The goal is to simulate the core educational workflows typically handled by comprehensive academic systems like Golestan.

---

## 🧰 Technologies and Concepts Used

- **ASP.NET Core MVC** – Layered architecture using the Model-View-Controller pattern
- **Entity Framework Core (ORM)** – Object-relational mapping for handling data persistence
- **Object-Oriented Programming (OOP)** – Class design, inheritance, and inter-object relationships
- **Git & GitHub** – Source control and collaboration
- **Relational Databases** – One of the following database engines is supported:
  - PostgreSQL
  - MySQL
  - Microsoft SQL Server

---

## 👥 User Roles and Permissions

### 🔐 Admin
- Add/remove courses, classes, instructors, and students
- Assign instructors to classes and students to courses
- Remove student/instructor assignments without causing conflicts
- View all user information with full details

### 🎓 Student
- View assigned courses and class schedules (e.g., start time, exam date)
- See grades and detailed academic history
- Cancel class enrollments (with pre-requisite checks)

### 👨‍🏫 instructor
- View assigned classes and enrolled students
- Grade students and manage their course participation
- Remove students from their class if needed

---

## 🛠 Functional Requirements

- ⏱ Prevent **time conflicts** in student and instructor class schedules
- 🏫 Avoid **location overlaps** in room assignments
- 🔐 Ensure **unique IDs** for instructors and students within each faculty
- 🔄 Automatically update course lists after assignments
- ✅ Enforce **prerequisite validation** before student enrollment
- ⚠️ Reject invalid inputs (e.g., grades outside the 0–20 range)
- 👤 Enforce **strict role-based access control** (e.g., students cannot create courses)

---

## 🧩 Core Entities

### instructor
- Includes ID, name, email, salary, hire date, and password

### Student
- Includes  ID, entry date, name, email and password

### Course
- Includes title, course code, final exam time and description

### Class
- Includes building, room number and capacity  

---

## 🔀 Git Workflow

- Create a **new branch** for each feature:
```bash
  git checkout -b feature/feature-name
```