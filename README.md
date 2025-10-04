# 🎉 HolidayApp

**HolidayApp** is a **.NET 8 Web API** that retrieves **public holidays** from the [Nager.Date API](https://date.nager.at/Api), 
stores them in a **SQL Server database**, and provides endpoints for analyzing, filtering, and comparing holidays across multiple countries.

---

## ⚙️ Getting Started

### 1️. Clone the repository

```bash
git clone https://github.com/Divyabharathi76/HolidayApp.git
   ```

2. **Configure the database:**
CREATE TABLE Holidays (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    CountryCode CHAR(3) NOT NULL,
    LocalName NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Date DATETIME NOT NULL,
    Types NVARCHAR(500) NOT NULL
);

   - Update the `appsettings.json` file with your SQL Server connection string.

   -- Index for last celebrated 3 holidays
    CREATE INDEX IX_Last3Holidays
    ON Holidays(CountryCode, [Date] DESC);
    Sorting a large table can be slow so adding the sorter here.

   -- Index for number of holidays in a year for certain countries
    CREATE INDEX IX_Holidays_Date_Country
    ON Holidays([Date], CountryCode);

3. **Restore dependencies and build the project:**
   ```bash
   cd HolidayApi
   dotnet restore
   dotnet build
   ```

4. **Start the API:**
   ```bash
   cd HolidayApi
   dotnet run
   ```

## Usage
- Test with Swagger

Open your browser and go to:
👉 http://localhost:{port-no}/swagger
*(Note: The port may differ depending on your environment or launch settings. Check your console output or `launchSettings.json` for the correct URL.)*

You can test all endpoints directly from Swagger UI.

| Endpoint                                                                | Description                                   |
| ------------------------------------------------------------------------| --------------------------------------------- |
| **POST** `/api/holidays/fetch/{countryCode}/{year}`                     | Fetches and stores holidays from Nager API    |
| **GET** `/api/holidays/last3/{name}`                                    | Returns the last 3 holidays by name           |
| **GET** `/api/holidays/nonweekendcount/{year}?countries={countryCodes}` | Lists non-weekend public holidays countrycodes|
| **GET** `/api/holidays/common/{year}?countries={countryCodes}`          | Shows common holidays for two countries       |

## Technologies Used

| Component                       | Description            |
| ------------------------------- | ---------------------- |
| **.NET 8 Web API**              | Backend framework      |
| **Entity Framework (non-core)** | ORM for SQL Server     |
| **SQL Server**                  | Database engine        |
| **Swagger / Swashbuckle**       | Interactive API docs   |
| **Nager.Date API**              | External data provider |
| **ILogger**                     | Logging framework      |


## Folder Structure 

HolidayApp/
│
├── HolidayApi/
│   ├── Controllers/
│   │   └── HolidaysController.cs
│   ├── Data/
│   │   └── HolidaysDbContext.cs
│   ├── Models/
│   │   └── Holidays.cs
│   ├── Services/
│   │   └── HolidayService.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── HolidayApi.csproj
│
├── HolidayApp.sln
|── global.json
└── README.md

## Logging
- Logging is handled using ILogger<HolidaysController>
- Writes to console by default

## 🎄 HolidayApp – A clean, production-ready .NET API for managing and analyzing holidays.
