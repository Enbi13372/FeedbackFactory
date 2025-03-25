Scrum Board:

https://github.com/users/Enbi13372/projects/1

[Unterrichtshandbuch_20241213.pdf](https://github.com/user-attachments/files/18209208/Unterrichtshandbuch_20241213.pdf)

Stand: 19.12.24 (https://github.com/user-attachments/files/18194584/Dokumentation.Sprint.Wochen.docx)
](https://5019.drive.bycs.de/external/project/24-25_ifa12a/Tauschordner/Projektdokumente/Gruppe%20C/Dokumentation%20Sprint%20Wochen%20.docx?app=ByCS-Office&fileId=e65358c5-7fd9-4305-b1af-85c628c817c5%2472d89ed5-98bc-43b2-9d96-085c654c51e8%214f769f5d-7077-407a-a0d0-b228ca0dc080&contextRouteName=files-spaces-generic&contextRouteParams.driveAliasAndItem=project/24-25_ifa12a/Tauschordner/Projektdokumente/Gruppe%20C&contextRouteQuery.fileId=e65358c5-7fd9-4305-b1af-85c628c817c5%2472d89ed5-98bc-43b2-9d96-085c654c51e8%21a6bfa215-d49b-4840-bf6a-39218b034f42&contextRouteQuery.sort-by=name&contextRouteQuery.sort-dir=asc)


Setting up FeedbackFactory:

1. A MYSQL Database needs to be created
2. The connectionstring can be added in "config.json"
3. The following tables have to be created:

   CREATE TABLE Users (
    Username VARCHAR(50) PRIMARY KEY,
    Password VARCHAR(100) NOT NULL,
    Role INT NOT NULL
);

   CREATE TABLE RegistrationKeys (
    `Key` VARCHAR(50) PRIMARY KEY
);

   CREATE TABLE Feedbackkeys (
    `Key` VARCHAR(50) PRIMARY KEY,
        UsesRemaining INT NOT NULL
);

   CREATE TABLE Classes (
    Teacher VARCHAR(255) NOT NULL,
    ClassName VARCHAR(255) NOT NULL,
    SchoolYear VARCHAR(9) NOT NULL,
    Department VARCHAR(255) NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    Grade INT NOT NULL,
    ClassSize INT NOT NULL
);



4. An initial Admin account has to be created:

   INSERT INTO Users (Username, Password, Role)
VALUES ('YourNameHere', 'YourPasswordHere', '1');









**Projekt Dokumentation**

---

# Deckblatt

## Entwicklung eines digitalen Feedbacksystems

**Projektteam:**  
Merle Goldschmidt, Yannik Durst, Niels Bodenschatz, Tobias Hoelzl  
**Klasse:** IFA 12  
**Schule:** Berufsschule Lichtenfels  
**Fach:** Informatik  
**Betreuender Lehrer:** [Name des Lehrers]  
**Abgabedatum:** [Datum]  

---

# Inhaltsverzeichnis

1. Einleitung  
2. Planung  
   2.1 Beschreibung des Auftrags  
   2.2 Rahmenbedingungen und Einflussfaktoren  
   2.3 Informationsbeschaffung  
   2.4 Zeitplanung  
   2.5 Muss-/Kann-Kriterien  
3. Durchführung  
   3.1 Aufgabenverteilung innerhalb des Teams  
   3.2 Sprint 1  
   3.3 Sprint 2  
   3.4 Sprint 3  
   3.5 Sprint 4  
4. Kontrolle und Bewertung der Ergebnisse  
   4.1 Darstellung des erzielten Ergebnisses  
   4.2 Aufgetretene Probleme und deren Lösungen  
   4.3 Zusammenarbeit im Team  
   4.4 Verbesserungspotenziale  
   4.5 Fazit  
5. Literaturverzeichnis  
6. Anhang  

---

# Einleitung

Im Rahmen des Schulprojekts wurde ein digitales Feedbacksystem entwickelt, das es Lehrkräften ermöglicht, anonymes Feedback von Schülerinnen und Schülern zu erhalten. Die Anwendung wurde mit Windows Presentation Foundation (WPF) und der Programmiersprache C# umgesetzt. 

Die Idee hinter diesem Projekt entstand aus dem Wunsch, eine effiziente und anonyme Möglichkeit zur Evaluation von Unterrichtsinhalten und Lehrmethoden zu schaffen. Dabei stand die einfache Bedienbarkeit der Software im Vordergrund. 

Die vorliegende Dokumentation beschreibt die verschiedenen Phasen der Entwicklung dieses Systems, angefangen bei der Planungsphase über die Implementierung bis hin zur abschließenden Bewertung der Ergebnisse. Zudem werden Herausforderungen erläutert, die im Projektverlauf auftraten, sowie mögliche Verbesserungen für zukünftige Versionen aufgezeigt. 

---

# Planung

## Beschreibung des Auftrags

Das Ziel des Projekts war die Entwicklung eines digitalen Feedbacksystems für den schulischen Gebrauch. Dieses System ermöglicht es Lehrkräften, Feedbackformulare zu erstellen, zu verwalten und auszuwerten. Schülerinnen und Schüler können anonym Rückmeldungen zu Unterrichtsinhalten und Methoden geben. 

Das System sollte eine intuitive Benutzeroberfläche besitzen und sowohl für Lehrkräfte als auch für Schülerinnen und Schüler leicht verständlich sein. Zudem war eine sichere Speicherung der Daten eine zentrale Anforderung des Projekts. 

## Rahmenbedingungen und Einflussfaktoren

Für die Umsetzung des Projekts wurde die Programmiersprache C# in Kombination mit Windows Presentation Foundation (WPF) verwendet. Die Speicherung der Daten erfolgt über eine MySQL-Datenbank. 

Als Projektmanagement-Methode wurde Scrum gewählt, um eine agile und iterative Entwicklung zu ermöglichen. Zur Versionsverwaltung kam GitHub zum Einsatz, um Änderungen am Code effizient nachzuverfolgen und kollaboratives Arbeiten zu ermöglichen. 

## Informationsbeschaffung

Die benötigten Informationen zur Umsetzung des Projekts wurden aus verschiedenen Quellen bezogen. Dazu gehören offizielle Dokumentationen zu WPF und MySQL, Online-Tutorials sowie Best Practices aus Softwareentwicklungsforen. Zudem wurden Fachbücher zur Softwareentwicklung genutzt, um fundierte Kenntnisse über die gewählten Technologien zu erlangen. 

## Zeitplanung

Das Projekt wurde in mehrere Sprints unterteilt, um eine strukturierte und effiziente Arbeitsweise zu gewährleisten. Jeder Sprint hatte bestimmte Aufgaben, die innerhalb eines festgelegten Zeitraums umgesetzt werden sollten. 

In der ersten Woche lag der Fokus auf der Einrichtung der Projektstruktur und der Datenbank. In der zweiten Woche wurde die Entwicklung der Benutzeranmeldung und Registrierung umgesetzt. Die dritte Woche war der Implementierung der Feedbackbögen sowie der Administrationsfunktionen gewidmet. In der vierten und letzten Woche wurden Optimierungen vorgenommen, Fehler behoben und finale Tests durchgeführt. 

## Muss-/Kann-Kriterien

Das Projekt wurde anhand von Muss- und Kann-Kriterien definiert. Zu den Muss-Kriterien gehörten die Implementierung der Feedbackformulare, die Benutzerverwaltung sowie die Datenbankanbindung. Zu den Kann-Kriterien zählten optionale Erweiterungen wie eine grafische Auswertung der Ergebnisse oder eine Rollenverwaltung für verschiedene Benutzergruppen. 

---

# Durchführung

## Aufgabenverteilung innerhalb des Teams

Das Team arbeitete nach der Scrum-Methode, wobei die Rollen des Scrum-Masters und des Product Owners wöchentlich wechselten. Die Entwicklung der Software übernahmen Niels Bodenschatz und Tobias Hoelzl, während die gesamte Gruppe für die Planung, Dokumentation und Qualitätssicherung verantwortlich war. 

## Sprint 1

Im ersten Sprint wurde das GitHub-Projekt eingerichtet, ein Scrum-Board erstellt und die grundlegende Struktur des Projekts definiert. Zudem wurde die Benutzerregistrierung entwickelt. 

## Sprint 2

Im zweiten Sprint wurde die Verbindung zur MySQL-Datenbank hergestellt. Außerdem wurde eine erste Version der Benutzeroberfläche für Lehrkräfte erstellt, die die Verwaltung der Feedbackformulare ermöglicht. Zudem wurden Code Reviews durchgeführt, um die Code-Qualität sicherzustellen. 

## Sprint 3

Der dritte Sprint konzentrierte sich auf die Verbesserung der Benutzeroberfläche. Zudem wurde eine Funktion zur Passwort-Zurücksetzung entwickelt und die automatische Generierung von Feedbackbögen implementiert. 

## Sprint 4

Im letzten Sprint lag der Fokus auf der Optimierung der Datenbankstruktur und der Implementierung der grafischen Auswertung der Feedback-Ergebnisse. Abschließend wurden Fehler behoben und das System ausführlich getestet. 

---

# Kontrolle und Bewertung der Ergebnisse

## Darstellung des erzielten Ergebnisses

Das entwickelte System ermöglicht es Lehrkräften, Feedbackbögen zu erstellen und die Rückmeldungen der Schülerinnen und Schüler anonymisiert auszuwerten. Zudem wurde eine Benutzerverwaltung integriert, die eine sichere Anmeldung und Verwaltung der Benutzer ermöglicht. 

## Aufgetretene Probleme und deren Lösungen

Während der Entwicklung traten einige Herausforderungen auf. Beispielsweise gab es zu Beginn Verbindungsprobleme mit der Datenbank, die durch eine Neuaufsetzung des Servers behoben wurden. Zudem mussten Inkonsistenzen im Design der Benutzeroberfläche korrigiert werden, um eine einheitliche Benutzererfahrung zu gewährleisten. 

## Zusammenarbeit im Team

Das Team arbeitete effizient zusammen und nutzte regelmäßige Meetings sowie Code Reviews zur Qualitätssicherung. Die Scrum-Methodik erwies sich als hilfreich, um Aufgaben klar zu definieren und Fortschritte strukturiert zu dokumentieren. 

## Verbesserungspotenziale

Für zukünftige Versionen könnte das System um eine erweiterte Benutzerrollen-Verwaltung ergänzt werden. Zudem könnten die grafischen Auswertungsmöglichkeiten weiter verbessert werden. 

## Fazit

Das Projekt wurde erfolgreich abgeschlossen. Die Kernfunktionen des Feedbacksystems sind voll funktionsfähig. Kleinere Optimierungen sind denkbar, jedoch wurde das Ziel des Projekts erreicht. 

---

# Literaturverzeichnis

- Müller, P., (2020), Einstieg in HTML und CSS, Rheinwerk.  
- https://docs.microsoft.com/en-us/dotnet/ (23.03.2025)  

---

# Anhang









Agile Entwicklung – Eine iterative und flexible Methode der Softwareentwicklung, die sich auf kontinuierliche Verbesserung und Zusammenarbeit im Team konzentriert.

Anonyme Feedbackfunktion – Eine Funktion des Systems, die es ermöglicht, dass Schülerinnen und Schüler Rückmeldungen geben, ohne identifiziert zu werden.

Benutzeroberfläche (UI, User Interface) – Die grafische Oberfläche der Software, über die Nutzer mit dem System interagieren.

Benutzerverwaltung – Ein Modul des Systems, das die Anmeldung, Registrierung und Verwaltung von Nutzerkonten ermöglicht.

Code Reviews – Eine Methode zur Qualitätssicherung, bei der Teammitglieder den Quellcode überprüfen, um Fehler zu identifizieren und Verbesserungen vorzuschlagen.

Datenbankanbindung – Die Verbindung zwischen der Software und einer MySQL-Datenbank zur Speicherung und Verwaltung der Feedbackdaten.

Feedbackbogen – Ein digitales Formular, mit dem Schülerinnen und Schüler Rückmeldungen zu Unterrichtsinhalten und Methoden geben können.

GitHub – Eine Plattform zur Versionskontrolle und kollaborativen Softwareentwicklung, die für das Projekt genutzt wurde.

Grafische Auswertung – Die visuelle Darstellung der gesammelten Feedbackdaten, um Lehrkräften eine bessere Analyse der Rückmeldungen zu ermöglichen.

Inkonsistenzen im Design – Uneinheitlichkeiten in der Benutzeroberfläche, die die Benutzerfreundlichkeit beeinträchtigen können.

Muss-/Kann-Kriterien – Eine Methode zur Priorisierung von Funktionen: „Muss“-Kriterien sind essenziell, während „Kann“-Kriterien optionale Erweiterungen darstellen.

MySQL – Ein relationales Datenbankmanagementsystem, das für die Speicherung der Feedbackdaten genutzt wurde.

Product Owner – Eine Rolle im Scrum-Team, die sich auf die Definition der Anforderungen und die Priorisierung der Aufgaben konzentriert.

Projektmanagement – Der Prozess der Planung, Organisation und Steuerung von Ressourcen zur Erreichung der Projektziele.

Qualitätssicherung – Maßnahmen zur Sicherstellung der Softwarequalität, einschließlich Tests und Code Reviews.

Scrum – Ein agiles Framework für das Projektmanagement, das iterative Entwicklungsphasen („Sprints“) und regelmäßige Meetings beinhaltet.

Scrum-Board – Ein visuelles Tool zur Verwaltung und Nachverfolgung von Aufgaben im Scrum-Prozess.

Scrum-Master – Eine Rolle im Scrum-Team, die für die Einhaltung der Scrum-Prinzipien und die Unterstützung des Teams verantwortlich ist.

Sprint – Eine Entwicklungsphase innerhalb des Scrum-Prozesses, in der bestimmte Aufgaben innerhalb eines festgelegten Zeitraums umgesetzt werden.

Versionsverwaltung – Die Nachverfolgung und Verwaltung von Änderungen im Quellcode, meist über Plattformen wie GitHub.

Windows Presentation Foundation (WPF) – Eine Technologie von Microsoft zur Entwicklung grafischer Benutzeroberflächen für Windows-Anwendungen.

Zeitanalyse/-planung – Die Organisation und Verteilung von Aufgaben über den gesamten Projektzeitraum hinweg.

Der Anhang enthält Screenshots der Benutzeroberfläche sowie Code-Snippets zur Veranschaulichung der Implementierung.



