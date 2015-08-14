module Chronos.Persistence.SaveFunctions
open System
open Chronos.Core
open System.IO
    
let shiftToCsv (WorkShift(Employee id, tcs)) = 
    let fileName = string id + "-" + DateTime.Today.ToShortDateString() + ".csv"
    let file = File.Open(fileName, FileMode.Append)
    use sw = new StreamWriter(file)
    for ClockedOut(start, ``end``, timeType) in tcs do
        let startTime = start.ToString("HH:mm")
        let endTime = ``end``.ToString("HH:mm")
        sw.WriteLine(startTime +  "," + endTime + "," + string timeType)
