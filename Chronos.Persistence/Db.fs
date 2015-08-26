module Chronos.Persistence.Db
open FSharp.Data.SqlClient
open FSharp.Data
open Chronos.Core
open System

module Sql =

    type private InsertShiftCommand = SqlCommandProvider<"SQL/InsertShift.sql", "name=chronosDb", SingleRow = true>
    type private InsertTimeCardCommand = SqlCommandProvider<"SQL/InsertTimeCard.sql", "name=chronosDb">

    let saveToSqlDb shift = 
        let (WorkShift (Employee(id), cards)) = shift
        use insertShiftCmd = new InsertShiftCommand()
        let shiftId = insertShiftCmd.Execute(id, DateTime.Today)
        match shiftId with
        | None    -> 
            failwith "Uh-oh something went very wrong here..."
        | Some id ->
            use insertTimeCardCmd = new InsertTimeCardCommand()
            for ClockedOut (start, ``end``, tt) in cards do
                let tag = 
                    match tt with
                    | Regular tag -> tag
                    | _           -> null
                insertTimeCardCmd.Execute(start.TimeOfDay, ``end``.TimeOfDay, tt.Id, id, tag)
                |> ignore