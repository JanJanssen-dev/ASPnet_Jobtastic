# ASPnet_Jobtastic Authorization Framework

## Inhaltsverzeichnis

1. [Überblick](#überblick)
2. [Komponenten](#komponenten)
   - [JobPostingOperations](#jobpostingoperations)
   - [JobPostingAuthorizationRequirement](#jobpostingauthorizationrequirement)
   - [JobPostingAuthorizationHandler](#jobpostingauthorizationhandler)
   - [JobPostingAuthorizationHelper](#jobpostingauthorizationhelper)
3. [Integration](#integration)
   - [Konfiguration in Program.cs](#konfiguration-in-programcs)
   - [Verwendung im Controller](#verwendung-im-controller)
4. [SweetAlert2-Integration](#sweetalert2-integration)
5. [Rollenbasierte Autorisierung](#rollenbasierte-autorisierung)
   - [Vorhandene Unterstützung](#vorhandene-unterstützung)
   - [Hinzufügen neuer Rollen](#hinzufügen-neuer-rollen)

## Überblick

Das Authorization Framework für ASPnet_Jobtastic implementiert eine ressourcenbasierte Autorisierung für JobPostings und integriert sich nahtlos in das ASP.NET Core Authorization Framework. Es bietet eine elegante Lösung für die Zugriffskontrolle basierend auf Eigentümerschaft und Freigaben sowie Unterstützung für rollenbasierte Autorisierung.

Hauptmerkmale:

- **Ressourcenbasierte Autorisierung**: Prüft Zugriffsrechte auf spezifische JobPostings
- **Eigentümerschaft und Freigaben**: Berücksichtigt JobSharing-Einstellungen für detaillierte Zugriffsrechte
- **Rollenbasierte Autorisierung**: Integration mit ASP.NET Core Identity Roles
- **SweetAlert2-Integration**: Benutzerfreundliche Fehlermeldungen bei Zugriffsverweigerung
- **Erweiterbarkeit**: Einfaches Hinzufügen neuer Operationen und Berechtigungen

## Komponenten

### JobPostingOperations

Diese statische Klasse definiert die verschiedenen Operationen, die auf JobPostings ausgeführt werden können.

```csharp
public static class JobPostingOperations
{
    public static readonly string View = "View";
    public static readonly string Edit = "Edit";
    public static readonly string Delete = "Delete";
    public static readonly string ManageSharing = "ManageSharing";
}
```

### JobPostingAuthorizationRequirement

Diese Klasse implementiert das `IAuthorizationRequirement`-Interface und definiert einen Parameter für die gewünschte Operation.

```csharp
public class JobPostingAuthorizationRequirement : IAuthorizationRequirement
{
    public string Operation { get; }

    public JobPostingAuthorizationRequirement(string operation)
    {
        Operation = operation;
    }
}
```

### JobPostingAuthorizationHandler

Der AuthorizationHandler implementiert die eigentliche Logik zur Prüfung der Berechtigungen. Er berücksichtigt:

1. Benutzerrollen (Admin hat alle Rechte)
2. Eigentümerschaft (Eigentümer hat alle Rechte)
3. Freigaben (Basierend auf den spezifischen Berechtigungen in JobSharing)

Die `HandleRequirementAsync`-Methode prüft die Berechtigungen in folgender Reihenfolge:

1. Ist der Benutzer ein Admin? → Zugriff erlauben
2. Ist der Benutzer der Eigentümer? → Zugriff erlauben
3. Für View-Operationen: Hat der Benutzer irgendeine Freigabe? → Zugriff erlauben
4. Für andere Operationen: Hat der Benutzer die entsprechenden Berechtigungen in der Freigabe?
   - Edit-Operation: CanEdit = true
   - Delete-Operation: CanDelete = true
   - ManageSharing: Nur der Eigentümer darf Freigaben verwalten

### JobPostingAuthorizationHelper

Diese Hilfsklasse vereinfacht die Verwendung der Autorisierung im Controller. Sie bietet eine Methode, die einen `IActionResult` zurückgibt, wenn die Autorisierung fehlschlägt, oder `null`, wenn sie erfolgreich ist.

Die Methode `AuthorizeJobPostingAsync`:
1. Ruft den `IAuthorizationService` auf, um die Autorisierung zu prüfen
2. Gibt `null` zurück, wenn die Autorisierung erfolgreich ist
3. Gibt einen Weiterleitungs-ActionResult zurück, wenn der Benutzer nicht angemeldet ist
4. Gibt einen ContentResult mit SweetAlert2-Script zurück, wenn der Benutzer angemeldet ist, aber keine Berechtigung hat

## Integration

### Konfiguration in Program.cs

Folgende Änderungen müssen in Program.cs vorgenommen werden:

```csharp
// Bei den using-Statements hinzufügen
using ASPnet_Jobtastic.Authorization;
using Microsoft.AspNetCore.Authorization;

// Füge Rollenunterstützung hinzu (bei Identity-Konfiguration)
builder.Services.AddDefaultIdentity<IdentityUser>(/* ... */)
    .AddRoles<IdentityRole>() // Rollen-Support hinzufügen
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Authorization Services hinzufügen
builder.Services.AddAuthorization(options => {
    // Eine allgemeine Policy für authentifizierte Benutzer
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Den AuthorizationHandler registrieren
builder.Services.AddScoped<IAuthorizationHandler, JobPostingAuthorizationHandler>();

// Den Authorization-Helper als Service registrieren
builder.Services.AddScoped<JobPostingAuthorizationHelper>();
```

### Verwendung im Controller

Im Controller wird der `JobPostingAuthorizationHelper` über Dependency Injection eingebunden und in den Action-Methoden verwendet:

```csharp
private readonly JobPostingAuthorizationHelper _authHelper;

public JobPostingController(
    ApplicationDbContext context, 
    ILogger<JobPostingController> logger,
    JobPostingAuthorizationHelper authHelper)
{
    _context = context;
    _logger = logger;
    _authHelper = authHelper;
}

// Beispiel für eine Action-Methode mit Autorisierung
public async Task<IActionResult> EditJob(int id)
{
    // Autorisierung prüfen
    var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, id, JobPostingOperations.Edit);
    if (authResult != null)
    {
        return authResult;
    }

    // Autorisierung erfolgreich, weiterer Code...
}
```

Für Methoden mit rollenbasierter Autorisierung kann das `[Authorize]`-Attribut mit Rollenangabe verwendet werden:

```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminDashboard()
{
    // Nur für Admins zugänglich
    return View();
}

[Authorize(Roles = "Admin,Manager")]
public IActionResult ManagerDashboard()
{
    // Für Admins und Manager zugänglich
    return View();
}
```
## SweetAlert2-Integration

Bei einer Zugriffsverweigerung wird ein SweetAlert2-Dialog angezeigt, der dem Benutzer eine benutzerfreundliche Meldung präsentiert und eine Schaltfläche zum Zurückkehren anbietet.

### Einbindung von SweetAlert2

Füge SweetAlert2 in dein Layout ein:

```html
<!-- In _Layout.cshtml -->
<head>
    <!-- Andere Stylesheet-Einträge -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>
</head>
```

### TempData-Benachrichtigungen

Für konsistente Benachrichtigungen kannst du auch TempData mit SweetAlert2 kombinieren.

## Rollenbasierte Autorisierung

### Vorhandene Unterstützung

Die Implementierung unterstützt bereits rollenbasierte Autorisierung. Der `JobPostingAuthorizationHandler` prüft, ob ein Benutzer in der Admin-Rolle ist und gewährt in diesem Fall alle Rechte:

```csharp
// Überprüfen, ob der Benutzer Admin ist
var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
if (isAdmin)
{
    context.Succeed(requirement);
    return;
}
```

### Hinzufügen neuer Rollen

#### 1. Rollen bei der Anwendungsinitialisierung erstellen

```csharp
// In Program.cs oder Startup.cs
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Rollen erstellen, falls sie nicht existieren
    string[] roleNames = { "Admin", "Manager", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
```

#### 2. Nutzung eines dedizierten RoleSeeder

Erstelle eine RoleSeeder-Klasse:

```csharp
public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Definiere deine Rollen
        string[] roleNames = { "Admin", "Manager", "User" };
        
        foreach (var roleName in roleNames)
        {
            // Rolle anlegen, falls sie nicht existiert
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
```

Und rufe ihn in Program.cs auf:

```csharp
// Am Ende von Program.cs, vor app.Run()
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ein Fehler ist beim Seeden der Rollen aufgetreten.");
    }
}
```

### Benutzer zu Rollen hinzufügen

Um Benutzer zu Rollen hinzuzufügen, verwende das UserManager:

```csharp
var user = await _userManager.FindByEmailAsync("benutzer@beispiel.de");
await _userManager.AddToRoleAsync(user, "Admin");
```

### Rollenverwaltungs-UI

Um eine Benutzeroberfläche für die Rollenverwaltung zu implementieren, erstelle einen Controller und Views. Der Controller sollte folgende Funktionen haben:

1. Anzeigen aller Benutzer mit ihren aktuellen Rollen
2. Bearbeiten der Rollen eines Benutzers 
3. Hinzufügen/Entfernen von Benutzern aus Rollen

```csharp
[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    // Implementiere Methoden für die Benutzerverwaltung:
    // - Index (Liste aller Benutzer mit Rollen)
    // - EditRoles (Bearbeiten der Rollen eines Benutzers)
}
```

### Erweitern des Authorization-Handlers für rollenbasierte Rechte

Du kannst die Logik im `JobPostingAuthorizationHandler` erweitern, um zusätzliche Rollen und deren Berechtigungen zu verwalten:

```csharp
// Überprüfen, ob der Benutzer Admin oder Manager ist
var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
var isManager = await _userManager.IsInRoleAsync(user, "Manager");

if (isAdmin)
{
    // Admins haben alle Rechte
    context.Succeed(requirement);
    return;
}

if (isManager)
{
    // Manager haben Ansichts- und Bearbeitungsrechte, aber keine Löschrechte
    if (requirement.Operation == JobPostingOperations.View || 
        requirement.Operation == JobPostingOperations.Edit)
    {
        context.Succeed(requirement);
        return;
    }
}
```

## Fehlerbehebung

### Problem: Benutzer erhält immer Zugriffsverweigerung

Mögliche Ursachen:
- Der Benutzer ist nicht der Eigentümer und hat keine Freigabe
- Die Freigabe existiert, aber die spezifischen Berechtigungen (CanEdit, CanDelete) sind nicht gesetzt
- Die Rollenprüfung funktioniert nicht korrekt

Lösung:
1. Überprüfe die JobSharing-Einträge in der Datenbank
2. Stelle sicher, dass der Benutzer in der richtigen Rolle ist
3. Überprüfe die Protokolle für Warnungen oder Fehler

### Problem: Authorization funktioniert nicht für alle Controller

Mögliche Ursachen:
- Der AuthorizationHandler ist nicht richtig registriert
- Die Controller verwenden nicht das `[Authorize]`-Attribut oder rufen den AuthorizationHelper nicht auf

Lösung:
1. Stelle sicher, dass der Handler in Program.cs registriert ist
2. Füge das `[Authorize]`-Attribut zu allen Controllern hinzu
3. Rufe den AuthorizationHelper in allen relevanten Action-Methoden auf