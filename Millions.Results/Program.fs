// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System
open System.IO
open System.Text.RegularExpressions
open System.Globalization

type ResType = Ok|NotOk|OkNoVote

[<EntryPoint>]
let main argv = 

    let g (t:Parser.Result)= 
                match t with
                |Parser.Result.Ok(_,_) -> ResType.Ok
                |Parser.Result.NotOk(_,_) -> ResType.NotOk
                |Parser.Result.OkNoVote(_,_) -> ResType.OkNoVote

    let dir = @"D:\MyData\Projects\Millions\Millions.Test\bin\Debug\Hist"
    let getDate fileName = 
            let d = Regex.Match(fileName,@"\(?<date>\d{2}_d{2}_d{2}).txt$").Groups.["date"].Value
            (DateTime.ParseExact(d,"dd_hh_mm",CultureInfo.InvariantCulture), fileName)

    let files = Directory.GetFiles(dir,"*", SearchOption.AllDirectories)
                //|> Array.map getDate
                |> Seq.map Parser.parseResults
                |> Seq.collect Seq.ofList
                |> Seq.groupBy g

    files |> Seq.iter (fun (t,res) -> printfn "%A -> %d" t (Seq.length res ))
                

    //printfn "%d" (Seq.length files)
    Console.ReadLine() |> ignore
    0 // return an integer exit code
