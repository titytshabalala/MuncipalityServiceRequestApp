# Municipal Services Application (MuncipalityServiceRequestApp)

A C# .NET Framework Windows Forms application that allows residents to report
municipal service issues (e.g. sanitation, roads, water, electricity).

## Requirements

- Windows 10/11
- Visual Studio 2019 or later (Community edition is fine)
- .NET Framework 4.7.2 or later (installed automatically with Visual Studio's
  ".NET desktop development" workload)

## How to Compile and Run

1. Open **MuncipalityServiceRequestApp.sln** in Visual Studio.
2. Wait for Visual Studio to finish loading/restoring the project.
3. Press **F5** (or click **Start**) to build and run the application.
   - Alternatively: **Build > Build Solution** (Ctrl+Shift+B), then run the
     produced `.exe` from `bin\Debug\`.
4. No external NuGet packages or database setup are required — the app uses
   only the built-in .NET Framework Windows Forms libraries and stores data
   in memory for the current session.

## How to Use the Application

1. **Main Menu** — on startup you'll see three options:
   - **Report Issues** — active and fully functional.
   - **Local Events and Announcements** — disabled (to be implemented later).
   - **Service Request Status** — disabled (to be implemented later).
2. Click **Report Issues** to open the reporting form.
3. Fill in:
   - **Location** — where the issue is.
   - **Category** — select from the dropdown (Sanitation, Roads, Water &
     Utilities, Electricity, Public Safety, Other).
   - **Description** — details of the issue.
   - **Attach Image/Document** (optional) — attach one or more supporting
     files via the file picker.
4. As you fill in the form, the **progress bar and message** below the
   attachment section update live to guide you toward completion.
5. Once Location, Category and Description are filled in, **Submit** becomes
   enabled. Click it to log the issue.
6. A confirmation popup will show your **reference number** and status
   (`Logged`). The form then clears so you can report another issue.
7. Click **Back to Main Menu** at any time to return to the main menu.

## Project Structure

| File                     | Purpose                                                          |
|--------------------------|-------------------------------------------------------------------|
| `Form1.cs`                | Main menu — three task options, navigation to Report Issues       |
| `ReportIssueForm.cs`      | Report Issues screen — form UI, validation, engagement feedback   |
| `Issue.cs`                | Data model representing a single reported issue                  |
| `IssueRepository.cs`      | In-memory `List<Issue>` used to store all reported issues         |

## Notes

- Reported issues are stored **in memory only** for the current run — they
  are not persisted to a file or database, and will be cleared when the
  application closes.
- Attached files are **referenced by their file path**, not copied into the
  project — make sure the original files remain in place if you need to
  re-open them later.
- The engagement strategy implemented here (live progress feedback while
  reporting) is a working preview of the **real-time status tracking**
  strategy selected in the Task 1 research document; the full loop will be
  completed once the Service Request Status page is implemented.
