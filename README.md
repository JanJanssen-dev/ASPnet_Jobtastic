# ASPnet_Jobtastic

## Inhaltsverzeichnis
- [�bersicht](#�bersicht)
- [Hauptfunktionen](#hauptfunktionen)
- [Technische Voraussetzungen](#technische-voraussetzungen)
- [Installation und Konfiguration](#installation-und-konfiguration)
  - [1. Projekt klonen oder herunterladen](#1-projekt-klonen-oder-herunterladen)
  - [2. Datenbankverbindung konfigurieren](#2-datenbankverbindung-konfigurieren)
  - [3. Datenbank migrieren](#3-datenbank-migrieren)
  - [4. Anwendung starten](#4-anwendung-starten)
- [Benutzung der Anwendung](#benutzung-der-anwendung)
  - [Erste Schritte](#erste-schritte)
  - [Administratorrolle einrichten](#administratorrolle-einrichten)
- [Autorisierungssystem](#autorisierungssystem)
- [Konfiguration](#konfiguration)
  - [Verbindungszeichenfolge](#verbindungszeichenfolge)
  - [Passwortrichtlinien](#passwortrichtlinien)
- [Rollenmanagement](#rollenmanagement)
- [Projektstruktur](#projektstruktur)
- [Erweiterbarkeit](#erweiterbarkeit)
- [Fehlerbehebung](#fehlerbehebung)
  - [Datenbankverbindungsfehler](#datenbankverbindungsfehler)
  - [Migrationsfehler](#migrationsfehler)
- [Lizenz](#lizenz)
- [Beitragen](#beitragen)
- [Mitwirkende](#mitwirkende)
- [Roadmap](#roadmap)
- [Kontakt](#kontakt)

## �bersicht

ASPnet_Jobtastic ist eine moderne ASP.NET Core-Webanwendung, die als Stellenportal dient. Die Anwendung erm�glicht es Benutzern, Stellenangebote zu erstellen, zu verwalten und mit anderen Benutzern zu teilen. Sie verf�gt �ber ein robustes Autorisierungssystem, Benutzerverwaltung mit unterschiedlichen Rollen und eine intuitive Benutzeroberfl�che.

![Jobtastic](https://drive.google.com/file/d/1EEh06PPH7NR3B_voyqiAec6zSSn8AaMm/view?usp=sharing)

## Hauptfunktionen

- **Benutzerverwaltung**: Registrierung, Anmeldung, Zwei-Faktor-Authentifizierung
- **Rollensystem**: Administrator-, Benutzer-, Moderator- und Support-Rollen
- **Stellenanzeigen-Verwaltung**: Erstellen, Bearbeiten, L�schen von Stellenangeboten
- **Freigabesystem**: Teilen von Stellenangeboten mit anderen Benutzern mit differenzierten Berechtigungen
- **Administrationsbereich**: Verwaltung von Benutzern und Rollen
- **Responsive Design**: Kompatibel mit Desktop- und Mobilger�ten (in arbeit)

## Technische Voraussetzungen

- **.NET 8.0**: Die Anwendung wurde mit .NET 8.0 entwickelt
- **Visual Studio**: Visual Studio 2022 oder h�her wird empfohlen
- **Microsoft SQL Server**: Die Anwendung verwendet standardm��ig MSSQL Server als Datenbank
  - **WICHTIG**: Die Anwendung ist NUR mit Microsoft SQL Server kompatibel, ohne weitere Anpassungen!
- **Internet-Verbindung**: F�r Bootstrap, jQuery und andere CDN-gehostete Ressourcen

## Installation und Konfiguration

### 1. Projekt klonen oder herunterladen

```bash
git clone https://github.com/yourusername/ASPnet_Jobtastic.git
cd ASPnet_Jobtastic
```

### 2. Datenbankverbindung konfigurieren

**WICHTIG**: Das Projekt erfordert Microsoft SQL Server. Andere Datenbanken werden nicht unterst�tzt.

�ffnen Sie die `appsettings.json`-Datei und passen Sie den Connection String an:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=ASPnet_Jobtastic;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;Application Intent=ReadWrite;MultipleActiveResultSets=True"
}
```

Ersetzen Sie:
- `YOUR_SERVER_NAME` mit dem Namen Ihres SQL Servers
- `YOUR_USERNAME` mit Ihrem SQL Server-Benutzernamen
- `YOUR_PASSWORD` mit Ihrem SQL Server-Passwort

### 3. Datenbank migrieren

In Visual Studio: �ffnen Sie die Package Manager Console und f�hren Sie folgende Befehle aus:

```
Update-Database
```

Alternativ �ber die Befehlszeile:

```bash
dotnet ef database update
```

### 4. Anwendung starten

In Visual Studio:
- Klicken Sie auf "Debuggen starten" oder dr�cken Sie F5

�ber die Befehlszeile:

```bash
dotnet run
```

Nach dem Start sollte die Anwendung unter https://localhost:7114 (oder http://localhost:5057) verf�gbar sein. 
!!! Port kann sich �ndern je nach IDE !!!

## Benutzung der Anwendung

### Erste Schritte

1. **Registrieren**: Erstellen Sie ein neues Benutzerkonto
2. **Anmelden**: Melden Sie sich mit Ihrem Konto an
3. **Stellenanzeige erstellen**: Klicken Sie auf "Neuen Job anlegen"
4. **Stellenanzeigen verwalten**: Verwalten Sie Ihre Anzeigen �ber die "Deine Inserate"-Seite

### Administratorrolle einrichten

Um einen Benutzer zum Administrator zu machen:

1. F�hren Sie eine SQL-Abfrage aus, um die Benutzer-ID zu ermitteln:
   ```sql
   SELECT Id FROM AspNetUsers WHERE UserName = 'gew�nschter_benutzername';
   ```

2. F�hren Sie eine SQL-Abfrage aus, um die Rollen-ID zu ermitteln:
   ```sql
   SELECT Id FROM AspNetRoles WHERE Name = 'Administrator';
   ```

3. F�hren Sie eine SQL-Abfrage aus, um den Benutzer zur Administratorrolle hinzuzuf�gen:
   ```sql
   INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('benutzer_id', 'administrator_rollen_id');
   ```

## Autorisierungssystem

Die Anwendung verf�gt �ber ein detailliertes Autorisierungssystem f�r Stellenanzeigen:

- **Eigent�mer**: Haben volle Kontrolle �ber ihre Stellenanzeigen
- **Freigabeberechtigungen**: K�nnen bestimmen, ob andere Benutzer Stellenanzeigen bearbeiten oder l�schen d�rfen
- **Administratoren**: Haben Zugriff auf alle Stellenanzeigen im System

Details zur Implementierung finden Sie im Verzeichnis `/Authorization` und in der Datei `README.md` in diesem Ordner.

## Konfiguration

### Verbindungszeichenfolge
Die Datenbankverbindung wird in `appsettings.json` konfiguriert:

```json
"ConnectionStrings": {
    "DefaultConnection": "Data Source=YourServerName;Initial Catalog=ASPnet_Jobtastic;User Id=YourUsername;Password=YourPassword;..."
}
```

### Passwortrichtlinien
Die Passwortrichtlinien k�nnen in `Program.cs` angepasst werden:

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 0;
});
```

## Rollenmanagement

Jobtastic unterst�tzt folgende Standardrollen:
- **Administrator**: Vollzugriff auf alle Funktionen
- **User**: Normaler Benutzer
- **Moderator**: Erweiterte Rechte (konfigurierbar)
- **Support**: Support-Mitarbeiter (konfigurierbar)

Rollen werden automatisch beim Anwendungsstart angelegt (siehe `Program.cs`):

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = new[] { "Administrator", "User", "Moderator", "Support" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
```

## Projektstruktur

- `/Areas/Identity`: Identity Framework-Komponenten f�r Authentifizierung
- `/Authorization`: Autorisierungslogik f�r die Ressourcenverwaltung
- `/Controllers`: MVC-Controller der Anwendung
- `/Data`: Datenbankkontext und Migrations-Dateien
- `/Models`: Datenmodelle der Anwendung
- `/Views`: MVC-Views f�r die Benutzeroberfl�che
- `/wwwroot`: Statische Ressourcen (CSS, JavaScript, Bilder)

## Erweiterbarkeit

Das System wurde mit Erweiterbarkeit im Fokus entwickelt:
- **JobPostingOperations** kann um neue Operationen erweitert werden
- **Autorisierungssystem** kann f�r weitere Entit�ten adaptiert werden
- **Benutzerrollen** k�nnen nach Bedarf hinzugef�gt werden

Um neue Operationen hinzuzuf�gen:
1. Erg�nzen Sie `JobPostingOperations.cs` um neue Operationstypen
2. Erweitern Sie den `JobPostingAuthorizationHandler` um die Logik f�r die neue Operation
3. Nutzen Sie die Operation in Ihren Controllern mit dem `JobPostingAuthorizationHelper`

## Fehlerbehebung

### Datenbankverbindungsfehler

- Stellen Sie sicher, dass Ihr SQL Server l�uft
- �berpr�fen Sie den Connection String in `appsettings.json`
- Stellen Sie sicher, dass der Benutzer die richtigen Berechtigungen hat

### Migrationsfehler

Bei Problemen mit der Datenbankmigration:

```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - siehe die LICENSE.txt-Datei f�r Details.

## Beitragen

Beitr�ge zum Projekt sind willkommen! Bitte folgen Sie dem Standard-Pull-Request-Prozess:
1. Fork des Repositories erstellen
2. Feature-Branch erstellen (`git checkout -b feature/AmazingFeature`)
3. �nderungen committen (`git commit -m 'Add some AmazingFeature'`)
4. Branch pushen (`git push origin feature/AmazingFeature`)
5. Pull Request erstellen

## Mitwirkende

- [Jan Janssen] - Urspr�nglicher Autor


## Roadmap

Geplante Funktionen f�r zuk�nftige Versionen:
- Integration von Bewerbungsmanagement
- Erweiterte Suchfunktionen
- API f�r externe Anwendungen
- Export/Import-Funktionen
- Mehrsprachige Unterst�tzung

## Kontakt

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im GitHub-Repository.