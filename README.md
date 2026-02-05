# Voya Manual Balance Updater

**Version:** 1.2.1  
**Date:** February 5, 2026  
**Developer:** Cave Arnold  
**AI Assistant:** Gemini

## Overview
The **Voya Manual Balance Updater** is a standalone Windows Forms application designed to simplify the process of tracking 401(k) assets. It serves as a manual bridge for updating the "Voya - Cave's 401(k)" account balance in the `Guyton-Klinger-Withdrawals` database when automated aggregators (like Plaid) encounter MFA issues.

The application features a high-contrast **Dark Mode** interface with centered, bright green text for maximum readability.

## Features
* **Real-time Balance Check:** Automatically connects to the database on startup to retrieve and display the current recorded balance.
* **Manual Entry:** Allows quick insertion of a new balance using raw text (supports currency symbols and commas).
* **Frequency Control:** Enforces a strict "One Entry Per Day" rule via the Stored Procedure to prevent accidental duplicate or multiple updates on the same date.
* **Dark Mode UI:** Custom dark charcoal background with high-visibility Lime Green text.

## Prerequisites
* **OS:** Windows 10/11
* **Framework:** .NET Desktop Runtime (6.0 or higher recommended)
* **Database:** SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
* **Target Database:** `Guyton-Klinger-Withdrawals`

## Database Setup
Ensure the following Stored Procedures exist in your database for the application to function correctly.

### 1. `usp_GetVoyaBalance` (Retrieves current value)
```sql
USE [Guyton-Klinger-Withdrawals]
GO
CREATE PROCEDURE [dbo].[usp_GetVoyaBalance]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 Balance
    FROM dbo.Balances
    WHERE Name = N'💰Voya - Cave''s 401(k) M'
    ORDER BY LastUpdate DESC;
END;
GO
```

### 2. `usp_AddManualBalance` (Inserts new value)
```sql
USE [Guyton-Klinger-Withdrawals]
GO
CREATE PROCEDURE [dbo].[usp_AddManualBalance]
    @BalanceInput NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SequenceID NVARCHAR(50) = N'4158071M';
    DECLARE @Name NVARCHAR(100)      = N'💰Voya - Cave''s 401(k) M';
    DECLARE @Type NVARCHAR(50)       = N'Account';
    DECLARE @Error NVARCHAR(256)     = N'Manual Update due to MFA issues with Plaid.';
    DECLARE @CleanBalance MONEY;

    -- Clean and convert the input
    SET @CleanBalance = TRY_CAST(REPLACE(REPLACE(@BalanceInput, '$', ''), ',', '') AS MONEY);

    -- CHECK 1: Ensure input is valid
    IF @CleanBalance IS NULL
    BEGIN
        RAISERROR('Invalid currency format provided.', 16, 1);
        RETURN;
    END

    -- CHECK 2: One Entry Per Day Rule
    -- We cast both the stored LastUpdate and GETDATE() to DATE to ignore the time component.
    IF EXISTS (
        SELECT 1 
        FROM dbo.Balances 
        WHERE Name = @Name 
          AND CAST(LastUpdate AS DATE) = CAST(GETDATE() AS DATE)
    )
    BEGIN
        RAISERROR('An entry for this account has already been made today.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Balances (SequenceID, Name, Balance, Type, Error)
    VALUES (@SequenceID, @Name, @CleanBalance, @Type, @Error);
END;
GO
```

## Installation & Build
1.  **Clone/Copy Source:** Save the provided `Program.cs` file into a new C# Console or WinForms project folder.
2.  **Dependencies:** Ensure the project references `System.Data.SqlClient` or `Microsoft.Data.SqlClient` via NuGet.
3.  **Compile:**
    * Open the project in Visual Studio or VS Code.
    * Run the build command:
    ```bash
    dotnet build
    ```
4.  **Run:** Execute the generated `.exe` from the `bin/Debug` or `bin/Release` folder.

## Usage
1.  Launch the application.
2.  Review the **Current Balance** displayed at the top of the window (fetched live from the database).
3.  Enter the new balance into the text box.
    * *Accepted formats:* `150200.50`, `150,200.50`, `$150,200.50`
4.  Click **UPDATE** or press **Enter**.
5.  A success message will appear, and the application will close automatically.
6.  *Note:* If you attempt to update the balance a second time on the same day, the application will display an error message.

## Version History
* **v1.0.0 (Jan 13, 2026):** Initial Release. Basic update functionality.
* **v1.1.0 (Feb 05, 2026):** Added current balance retrieval.
* **v1.2.0 (Feb 05, 2026):** UI overhaul (Dark Mode, Lime Green text).
* **v1.2.1 (Feb 05, 2026):** Updated database logic to enforce strict "One Entry Per Day" validation.