open System
open System.Windows.Forms
open Chronos.Data
open Chronos.Persistence.SaveFunctions
open Chronos.Core
open Chronos.Persistence.Db.Sql
open Chronos.Persistence.Db
open System.Drawing
open System.Configuration

do
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false

let employee = 
    ConfigurationManager.AppSettings.["employeeId"]
    |> int
    |> Employee

type Message = 
    | OpenTimeCard of TimeType
    | FetchTimeCards of AsyncReplyChannel<TimeCard list>

[<Literal>]
let radioBtnOffset = 20

let regularTimeRadioBtn = 
    new RadioButton(Text = "Regular", Left = 50, Top = 30, Checked = true,
                    Enabled = false)

let breakTimeRadioBtn = 
    new RadioButton(Text = "Break", 
                    Top = regularTimeRadioBtn.Top + radioBtnOffset,
                    Left = regularTimeRadioBtn.Left,
                    Enabled = false)
let lunchTimeRadioBtn = 
    new RadioButton(Text = "Lunch", 
                    Top = breakTimeRadioBtn.Top + radioBtnOffset,
                    Left = regularTimeRadioBtn.Left,
                    Enabled = false)

//let getCheckedTimeType() = 
//    let (_,res) = 
//        [ regularTimeRadioBtn, 1
//          breakTimeRadioBtn,   2 
//          lunchTimeRadioBtn,   3 ]
//        |> List.find (fun (btn,_) -> btn.Checked)
//    res

let clockInBtn = 
    new Button(Text = "Clock in", 
               Left = 50, 
               Top = 100)

let enableRadioBtns() =
    [regularTimeRadioBtn; lunchTimeRadioBtn; breakTimeRadioBtn]
    |> List.iter (fun btn -> btn.Enabled <- true)

let form = new Form(Width = 200, Height = 200, BackColor = Color.GreenYellow)
//let timeTypeChanged =
//    [ regularTimeRadioBtn, TimeType.Regular 
//      breakTimeRadioBtn,   TimeType.Break 
//      lunchTimeRadioBtn,   TimeType.Lunch ]
//    |> List.map (fun (btn, tt) -> 
//        btn.CheckedChanged |> Observable.map (fun _ -> tt)
//                           |> Observable.filter (fun _ -> btn.Checked))
//    |> List.reduce (Observable.merge)

let agent = MailboxProcessor.Start(fun mbox -> 
    let rec loop st = async { 
        let! msg = mbox.Receive()
        match msg with
        | FetchTimeCards replyChannel -> 
            replyChannel.Reply(st)
        | OpenTimeCard tt ->
            // Stamp out the previous card if it exists
            let st = 
                match st with
                | card::tl -> 
                    let clockedCard = card |> stampCard 
                    clockedCard::tl
                | _ -> [] 
            let blankCard = Unclocked tt |> stampCard
            return! loop (blankCard::st) }
    loop [])

let radioBtnChecked (radioBtn:RadioButton) =
    radioBtn.CheckedChanged 
    |> Observable.filter (fun _ -> radioBtn.Checked)


let timeColor =
    function
    | Regular _ -> Color.Green
    | Break     -> Color.Orange
    | Lunch     -> Color.Red


do
    let getRegularTag() = 
        Microsoft.VisualBasic.Interaction.InputBox("Enter tag name:")

    let regularSet =
        regularTimeRadioBtn |> radioBtnChecked
                            |> Observable.map 
                                (ignore >> getRegularTag >> Some)
    
    let timeTypeChanged = 
        [ regularTimeRadioBtn, fun () -> Regular <| getRegularTag()
          breakTimeRadioBtn,   fun () -> Break 
          lunchTimeRadioBtn,   fun () -> Lunch ]
        |> List.map (fun (btn, f) -> 
            btn |> radioBtnChecked
                |> Observable.map (ignore >> f))
        |> List.reduce (Observable.merge)

    clockInBtn.Click.Add (fun _ -> 
        enableRadioBtns()
        clockInBtn.Visible <- false
        let tt = getRegularTag() |> Regular
        OpenTimeCard tt |> agent.Post)

    timeTypeChanged |> Observable.add (fun tt ->
        form.BackColor <- timeColor tt
        OpenTimeCard tt |> agent.Post)
    
    form.FormClosing.Add(fun ev -> 
       let timeCards = 
           agent.PostAndReply (fun replyChannel -> FetchTimeCards replyChannel)
       match timeCards with
       | [] -> ()
       | hd::tl ->
           let clockedCard = hd |> stampCard
           let shift = createWorkShift employee <| clockedCard::tl
           shift |> saveToSqlDb)
       
[<STAThread>]
do 
    ([clockInBtn; regularTimeRadioBtn; 
      breakTimeRadioBtn; lunchTimeRadioBtn] : Control list)
    |> List.iter (form.Controls.Add)
    Application.Run(form)


    