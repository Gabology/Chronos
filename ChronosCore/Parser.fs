namespace ChronosCore

open System
open Microsoft.FSharp.Reflection
open System.Xml.Linq

module Parser =
    let ($) str = XName.Get(str)
    [<CompiledName("Parse")>]
    let parse(name, (date: DateTime), (hours,minutes), billType, descr) =
        let xml = 
            XElement(($)"Timeblock", 
                [| XElement(($)"Name", string name) 
                   XElement(($)"Date", date.ToShortDateString()) 
                   XElement(($)"Duration", string <| TimeSpan(hours, minutes, 0))
                   XElement(($)"BillingTime", string billType)
                   XElement(($)"Description", string descr)  |])
        let name = 
            date.ToShortDateString()
                .Replace("-","") + "-" + (string name) + ".xml"
        xml.Save(name)