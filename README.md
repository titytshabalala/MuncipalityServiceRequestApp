# Municipal Services Application

`MuncipalityServiceRequestApp` is a Windows Forms application for the City of Tshwane. Residents can report municipal service issues, track request progress, and view local events and announcements. Municipal administrators can manage requests, announcements, and the service-area road network from the Admin Portal.

## Requirements

- Windows 10 or Windows 11
- Visual Studio 2022 or later with the **.NET desktop development** workload
- .NET Framework 4.8 Developer Pack
- No external database server or third-party package is required

## Build and run

1. Clone the repository:

   ```powershell
   git clone https://github.com/titytshabalala/MuncipalityServiceRequestApp.git
   cd MuncipalityServiceRequestApp
   ```

2. Open `MuncipalityServiceRequestApp.slnx` in Visual Studio.
3. Select the `MuncipalityServiceRequestApp` project as the startup project if necessary.
4. Build the solution with **Build > Build Solution** or `Ctrl+Shift+B`.
5. Press **F5** or select **Start** to run the application.

The compiled executable is produced under the project's `bin\Debug\net48` or `bin\Release\net48` directory, depending on the selected configuration.

## Application features

### Resident features

- **Report Issues** — Report a problem, add attachments, and receive a unique service request number.
- **Local Events and Announcements** — Search public notices and community announcements by keyword, category, or location.
- **Service Request Status** — Find a request, filter by current stage, and view request history, priority, location, and service area.

### Administrator features

Open **Admin Portal** from the main menu. It provides:

- **Dashboard** — Total, open, and completed requests grouped by stage, service category, and priority.
- **Manage Service Requests** — Search and filter requests, assign them to a team or officer, move them to the next service stage, and add progress notes.
- **Events and Announcements** — Add, edit, and remove public notices and community events.
- **Service Zone Network** — Add service areas and roads, then view the minimum road network using readable calculation options.

## Data persistence

Application data is saved as JSON in the current user's local application data directory:

```text
%LOCALAPPDATA%\MuncipalityServiceRequestApp\municipal-data.json
```

Requests, request history, events, service areas, and roads are loaded when the application starts and saved after administrative changes and when the application exits. The data file is local to the Windows user account and is not shared between computers or users.

## Project structure

| File or directory | Purpose |
|---|---|
| `Form1.cs` | Main menu and navigation. |
| `AdminPortalForm.cs` | Administrator dashboard and management tools. |
| `ReportIssueForm.cs` | Resident service-request submission form. |
| `ServiceStatusForm.cs` | Request tracking and service-network views. |
| `LocalEventsForm.cs` | Resident events and announcements view. |
| `Issue.cs` / `IssueRepository.cs` | Service-request model and repository. |
| `MunicipalEvent.cs` / `EventRepository.cs` | Event model and repository. |
| `DataStructures/` | AVL tree, priority heap, and service-zone graph implementations. |
| `AppDataStore.cs` | JSON serialization, loading, and saving. |
| `AppTheme.cs` | Shared layout, typography, colours, and control styling. |

## Technical notes

- The application targets `.NET Framework 4.8` and uses C# 7.3 language features.
- JSON persistence uses the .NET Framework `DataContractJsonSerializer`.
- The service-request reference index uses an AVL tree, priority ordering uses a binary heap, and service-area roads use a weighted graph.
- The Admin Portal is an administrative interface, not a multi-user authentication or authorisation system. Add authentication before deploying it in a shared or production environment.

## Git housekeeping

Build output, Visual Studio user files, local persistence data, and temporary files should remain untracked. Review `.gitignore` before committing:

```powershell
git status
git diff
```
