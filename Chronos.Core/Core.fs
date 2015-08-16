module Chronos.Core
open System

type TimeType =
    | Regular
    | Break
    | Lunch
    member x.Id =
        match x with
        | Regular -> 1
        | Break   -> 2
        | Lunch   -> 3

type Employee = Employee of int

type TimeCard =
    | Unclocked of TimeType
    | ClockedIn of DateTime * TimeType
    | ClockedOut of DateTime * DateTime * TimeType

type WorkShift = WorkShift of Employee * TimeCard list

let stampCard timeCard =
    match timeCard with
    | Unclocked timeType -> ClockedIn (DateTime.Now, timeType)
    | ClockedIn (startTime, timeType) -> ClockedOut (startTime, DateTime.Now, timeType)
    | _ -> failwith "This timecard has been clocked out already"

let createWorkShift (employee:Employee) timeCards =
    let allClockedOut = 
        timeCards |> List.forall 
            (function ClockedOut _ -> true | _ -> false) 
    if allClockedOut then 
        WorkShift (employee, timeCards)
    else 
        failwith "A work shift cannot contain unclocked cards"

let clockIn timeType timeCards =
    match timeCards with
    | ClockedOut _::_ | [] ->
        let timeCard = Unclocked timeType |> stampCard
        timeCard::timeCards
    | _ -> 
        invalidArg "timeCards" "You have not clocked out your active time card"

let clockOut timeCards =
    match timeCards with
    | [] -> failwith "No time cards provided"
    | hd::tl -> 
        let clockedOutTc = hd |> stampCard
        clockedOutTc::tl