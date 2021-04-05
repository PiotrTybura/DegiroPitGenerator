# DegiroPitGenerator

## How to run the release
1. Download the latest Release from Github
2. Put your Degiro Username and Password into appsettings.json
3. Start DegiroPitGenerator.exe with year param (for instance `2020`)
4. After a couple of seconds the Json file with all crucial PIT-38 fields opens

## What if I don't want to put my credentials into this app?
Fair enough you can do this, but it requires a few more manual steps. Please:
1. Download `Transactions.csv` from Degiro. It can be found under "Activities -> Transactions -> Download". Please change the report start date to "any day before you created Degiro account" and end date to today.
2. Download `Account.csv` from Degiro. It can be found under "Activities -> Account Overview -> Download". Please change the report start date to the 1st of January of the tax year (so the year that you put as the first parameter to the application) and the end day to the 31st of December (the same year).
3. Update application.json (`DegiroCsvOverride`) to indiciate the path for these file reports. Below you can find an example how it may look like:
```
  "DegiroCsvOverride": {
    "UseLocalCsvs": true,
    "TransactionsCsvPath": "./Transactions.csv",
    "CashOperationsCsvPath": "./Account.csv"
  }
```
4. Run the application

## How to compile & run?
The process is almost the same as running the release. The only difference is you need to download the source code instead of release and compile it.

## Motivation
Degiro does not provide PIT-8C (the Polish tax Report) so if you are a Polish citizen then you need to calculate the tax by yourself. There is a general Degiro yearly report that some people use for this purpose, however it provides exchange rates that are not accepted by the Polish Tax Office.

## More info
Can be found on Wiki

## Special Thanks to:
- My wife Etka
- Friendly Exchange Circle
