# ASPnet_Jobtastic

## Übersicht

ASPnet_Jobtastic ist eine moderne ASP.NET Core-Webanwendung, die als Stellenportal dient. Die Anwendung ermöglicht es Benutzern, Stellenangebote zu erstellen, zu verwalten und mit anderen Benutzern zu teilen. Sie verfügt über ein robustes Autorisierungssystem, Benutzerverwaltung mit unterschiedlichen Rollen und eine intuitive Benutzeroberfläche.

![Jobtastic](https://placeholder-image-url.com/jobtastic-screenshot.png)

## Hauptfunktionen

- **Benutzerverwaltung**: Registrierung, Anmeldung, Zwei-Faktor-Authentifizierung
- **Rollensystem**: Administrator-, Benutzer-, Moderator- und Support-Rollen
- **Stellenanzeigen-Verwaltung**: Erstellen, Bearbeiten, Löschen von Stellenangeboten
- **Freigabesystem**: Teilen von Stellenangeboten mit anderen Benutzern mit differenzierten Berechtigungen
- **Administrationsbereich**: Verwaltung von Benutzern und Rollen
- **Responsive Design**: Kompatibel mit Desktop- und Mobilgeräten

## Technische Voraussetzungen

- **.NET 8.0**: Die Anwendung wurde mit .NET 8.0 entwickelt
- **Visual Studio**: Visual Studio 2022 oder höher wird empfohlen
- **Microsoft SQL Server**: Die Anwendung verwendet standardmäßig SQL Server als Datenbank
  - **WICHTIG**: Die Anwendung ist NUR mit Microsoft SQL Server kompatibel!
- **Internet-Verbindung**: Für Bootstrap, jQuery und andere CDN-gehostete Ressourcen

## Installation und Konfiguration

### 1. Projekt klonen oder herunterladen

```bash
git clone https://github.com/yourusername/ASPnet_Jobtastic.git
cd ASPnet_Jobtastic
```

### 2. Datenbankverbindung konfigurieren

**WICHTIG**: Das Projekt erfordert Microsoft SQL Server. Andere Datenbanken werden nicht unterstützt.

Öffnen Sie die `appsettings.json`-Datei und passen Sie den Connection String an:

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

In Visual Studio: Öffnen Sie die Package Manager Console und führen Sie folgende Befehle aus:

```
Update-Database
```

Alternativ über die Befehlszeile:

```bash
dotnet ef database update
```

### 4. Anwendung starten

In Visual Studio:
- Klicken Sie auf "Debuggen starten" oder drücken Sie F5

Über die Befehlszeile:

```bash
dotnet run
```

Nach dem Start sollte die Anwendung unter https://localhost:7114 (oder http://localhost:5057) verfügbar sein.

## Benutzung der Anwendung

### Erste Schritte

1. **Registrieren**: Erstellen Sie ein neues Benutzerkonto
2. **Anmelden**: Melden Sie sich mit Ihrem Konto an
3. **Stellenanzeige erstellen**: Klicken Sie auf "Neuen Job anlegen"
4. **Stellenanzeigen verwalten**: Verwalten Sie Ihre Anzeigen über die "Deine Inserate"-Seite

### Administratorrolle einrichten

Um einen Benutzer zum Administrator zu machen:

1. Führen Sie eine SQL-Abfrage aus, um die Benutzer-ID zu ermitteln:
   ```sql
   SELECT Id FROM AspNetUsers WHERE UserName = 'gewünschter_benutzername';
   ```

2. Führen Sie eine SQL-Abfrage aus, um die Rollen-ID zu ermitteln:
   ```sql
   SELECT Id FROM AspNetRoles WHERE Name = 'Administrator';
   ```

3. Führen Sie eine SQL-Abfrage aus, um den Benutzer zur Administratorrolle hinzuzufügen:
   ```sql
   INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('benutzer_id', 'administrator_rollen_id');
   ```

## Autorisierungssystem

Die Anwendung verfügt über ein detailliertes Autorisierungssystem für Stellenanzeigen:

- **Eigentümer**: Haben volle Kontrolle über ihre Stellenanzeigen
- **Freigabeberechtigungen**: Können bestimmen, ob andere Benutzer Stellenanzeigen bearbeiten oder löschen dürfen
- **Administratoren**: Haben Zugriff auf alle Stellenanzeigen im System

Details zur Implementierung finden Sie im Verzeichnis `/Authorization` und in der Datei `README.md` in diesem Ordner.

## Projektstruktur

- `/Areas/Identity`: Identity Framework-Komponenten für Authentifizierung
- `/Authorization`: Autorisierungslogik für die Ressourcenverwaltung
- `/Controllers`: MVC-Controller der Anwendung
- `/Data`: Datenbankkontext und Migrations-Dateien
- `/Models`: Datenmodelle der Anwendung
- `/Views`: MVC-Views für die Benutzeroberfläche
- `/wwwroot`: Statische Ressourcen (CSS, JavaScript, Bilder)

## Fehlerbehebung

### Datenbankverbindungsfehler

- Stellen Sie sicher, dass Ihr SQL Server läuft
- Überprüfen Sie den Connection String in `appsettings.json`
- Stellen Sie sicher, dass der Benutzer die richtigen Berechtigungen hat

### Migrationsfehler

Bei Problemen mit der Datenbankmigration:

```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - siehe die LICENSE.txt-Datei für Details.

## Mitwirkende

- [Opa Hans Dampf] - Ursprünglicher Autor
- [Weitere Mitwirkende]

## Kontakt

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im GitHub-Repository.