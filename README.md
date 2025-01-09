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
    ClassName VARCHAR(255) NOT NULL PRIMARY KEY,
    SchoolYear VARCHAR(9) NOT NULL,
    Department VARCHAR(255) NOT NULL,
    Class VARCHAR(255) NOT NULL,
    Grade INT NOT NULL,
    ClassSize INT NOT NULL
);



4. An initial Admin account has to be created:

   INSERT INTO Users (Username, Password, Role)
VALUES ('YourNameHere', 'YourPasswordHere', '1');


